using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace WistBot
{
    public static class UserStateManager
    {
        public enum UserState
        {
            SettingListName,
            SettingItemName,
            SettingDescription,
            SettingLink,
            SettingMedia,
            Free
        }

        private static Dictionary<long, (UserState, object)> UserStates = new Dictionary<long, (UserState, object)>();
        public static void SetState(long userId, UserState state, object objectToupdate = null)
        {
            if (UserStates.ContainsKey(userId))
            {
                if (objectToupdate is null)
                {
                    UserStates[userId] = (state, UserStates[userId].Item2);
                }
                else
                    UserStates[userId] = (state, objectToupdate);
            }
            else
            {
                UserStates.Add(userId, (state, objectToupdate));
            }
            return;
        }

        public static (UserState, object) GetState(long userId)
        {
            if (UserStates.ContainsKey(userId))
            {
                return UserStates[userId];
            }
            else
            {
                return (UserState.Free, null);
            }
        }

        public static void RemoveState(long userId)
        {
            if (UserStates.ContainsKey(userId))
            {
                UserStates.Remove(userId);
            }
            return;
        }

        public static void ClearStates()
        {
            UserStates.Clear();
            return;
        }

        public static bool IsUserInState(long userId, UserState state)
        {
            return GetState(userId).Item1 == state;
        }

        public static bool UserHasState(long userId)
        {
            return UserStates.ContainsKey(userId);
        }

        internal static async Task HandleStates(long userId, Message message, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database _database)
        {
            var state = GetState(userId).Item1;
            var ObjectToUpdate = GetState(userId).Item2;
            if(message is null)
            {
                return;
            }
            if (message.Text is not null)
            {
                if (message.Text.StartsWith("@"))
                {
                    await BotActions.ShowUserLists(message, bot, token, _localization, _database);
                    return;
                }
                if (message.Text.StartsWith("/"))
                {
                    await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.NameCantStartWithSlash), cancellationToken: token);
                    return;
                }
            }

            switch (state)
            {
                case UserState.SettingListName:
                    var newListName = message.Text;
                    if (string.IsNullOrEmpty(newListName))
                    {
                        await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.NameCantBeEmpty), cancellationToken: token);
                        break;
                    }
                    if (ObjectToUpdate is WishList list)
                    {
                        list.Name = newListName;
                        _database.UpdateWishList(userId, list.Id, list);
                    }
                    else
                    {
                        var newList = new WishList { Name = newListName };
                        _database.AddWishList(userId, newList);
                    }

                    await BotActions.ListsAction(message, bot, token, _localization, _database);
                    break;

                case UserState.SettingItemName:
                    var itemName = message.Text;
                    if (itemName == null)
                    {
                        await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.NameCantBeEmpty), cancellationToken: token);
                        break;
                    }
                    if (ObjectToUpdate is WishList wishList)
                    {
                        var baseName = itemName;
                        var counter = 1;
                        while (wishList.Items.Any(x => x.Name == itemName))
                        {
                            itemName = $"{baseName}{counter}";
                            counter++;
                        }
                        wishList.Items.Add(new WishListItem(wishList.Name) { Name = itemName });
                        _database.UpdateWishList(userId, wishList.Id, wishList);
                        await BotActions.ShowList(message, bot, token,  _database, _localization, wishList.Name);
                    }
                    else if (ObjectToUpdate is WishListItem wishListItem)
                    {
                        var baseName = itemName;
                        var counter = 1;
                        var parentListName = wishListItem.ListName;

                        var parentList = _database.GetWishList(userId, parentListName);
                        while (parentList.Items.Any(x => x.Name == itemName))
                        {
                            itemName = $"{baseName}{counter}";
                            counter++;
                        }

                        wishListItem.Name = itemName;
                        _database.UpdateItem(userId, parentListName, wishListItem);
                        await BotActions.ShowList(message, bot, token, _database, _localization, parentListName);
                    }
                    break;
                case UserState.SettingDescription:
                    var description = message.Text;
                    if (description == null)
                    {
                        await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.DescriptionCantBeEmpty), cancellationToken: token);
                        break;
                    }
                    if (ObjectToUpdate is WishListItem)
                    {
                        ((WishListItem)ObjectToUpdate).Description = description;
                        _database.UpdateItem(userId, ((WishListItem)ObjectToUpdate).ListName, (WishListItem)ObjectToUpdate);
                        await BotActions.ShowList(message, bot, token, _database, _localization, ((WishListItem)ObjectToUpdate).ListName);
                    }
                    break;
                case UserState.SettingLink:
                    if (!Uri.IsWellFormedUriString(message.Text, UriKind.Absolute))
                    {
                        await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.InvalidLink), cancellationToken: token);
                        break;
                    }
                    var link = message.Text;
                    if (ObjectToUpdate is WishListItem)
                    {
                        ((WishListItem)ObjectToUpdate).Link = link;
                        _database.UpdateItem(userId, ((WishListItem)ObjectToUpdate).ListName, (WishListItem)ObjectToUpdate);
                        await BotActions.ShowList(message, bot, token, _database, _localization, ((WishListItem)ObjectToUpdate).ListName);
                    }
                    break;
                case UserState.SettingMedia:
                    if (message.Document != null)
                    {
                        await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.DocumentNotSupported), cancellationToken: token);
                        break;
                    }
                    if (message.Photo != null)
                    {
                        if (ObjectToUpdate is WishListItem wish)
                        {
                            if (wish.Video != null)
                            {
                                wish.Video = null;
                                break;
                            }
                            wish.Photo = message.Photo[0];
                            _database.UpdateItem(userId, wish.ListName, wish);
                            await BotActions.ShowList(message, bot, token, _database, _localization, wish.ListName);
                        }
                        break;
                    }
                    if (message.Video != null)
                    {
                        if(message.Video.Duration > 60)
                        {
                            await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.VideoTooLong), cancellationToken: token);
                            break;
                        }
                        if (ObjectToUpdate is WishListItem wish)
                        {
                            if (wish.Photo != null)
                            {
                                Console.WriteLine(2);

                                wish.Photo = null;
                                break;
                            }
                            wish.Video = message.Video;
                            Console.WriteLine(wish.Video.Duration);

                            _database.UpdateItem(userId, wish.ListName, wish);
                            await BotActions.ShowList(message, bot, token, _database, _localization, wish.ListName);
                        }
                        break;
                    }

                    await BotActions.ShowList(message, bot, token, _database, _localization, ((WishListItem)ObjectToUpdate).ListName);

                    break;
                case UserState.Free:
                    
                    break;
            }
            RemoveState(userId);
        }
    }
}
