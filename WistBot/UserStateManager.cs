using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Services;
using WistBot.States;
using WistBot.UserStates;

namespace WistBot
{
    public class UserStateManager
    {
        private readonly Dictionary<long, IUserStateHandler> _userStates = new();

        public void SetState(long userId, IUserStateHandler stateHandler)
        {
            _userStates[userId] = stateHandler;
        }

        public async Task HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, WishListItemsService wishListItemsService)
        {
            if (_userStates.TryGetValue(userId, out var stateHandler))
            {
                await stateHandler.HandleStateAsync(userId, message, bot, token, localization, wishListsService, wishListItemsService);
                if (stateHandler is ViewingListState)
                {
                    return;
                }
                _userStates.Remove(userId);
            }
        }

        public void RemoveState(long userId)
        {
            _userStates.Remove(userId);
        }

        public bool UserHasState(long userId)
        {
            return _userStates.ContainsKey(userId);
        }

        public void ClearStates()
        {
            _userStates.Clear();
        }

       /* internal static async Task HandleStates(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService _localization, WishListsService _wishListsService, WishListItemsService _wishListItemsService)
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
                    await BotActions.ShowUserLists(message, token, _localization);
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
                    if (string.IsNullOrWhiteSpace(newListName))
                    {
                        await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.NameCantBeEmpty), cancellationToken: token);
                        break;
                    }
                    if (ObjectToUpdate is WishListEntity list)
                    {
                        await _wishListsService.Update(list.Id, newListName, list.IsPublic);
                    }
                    else
                    {
                        await _wishListsService.Add(newListName, false, userId);
                    }

                    await BotActions.ListsAction(message, token, _localization);
                    break;

                case UserState.SettingItemName:
                    var itemName = message.Text;
                    if (string.IsNullOrWhiteSpace(itemName))
                    {
                        await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.NameCantBeEmpty), cancellationToken: token);
                        break;
                    }
                    if (ObjectToUpdate is WishListEntity wishList)
                    {
                        var baseName = itemName;
                        var counter = 1;
                        while (wishList.Items.Any(x => x.Name == itemName))
                        {
                            itemName = $"{baseName}{counter}";
                            counter++;
                        }
                        await _wishListItemsService.Add(new WishListItemEntity() { Name = itemName, ListId = wishList.Id});
                        await BotActions.ShowList(message, token, _localization, wishList.Name);
                    }
                    else if (ObjectToUpdate is WishListItemEntity wishListItem)
                    {
                        var baseName = itemName;
                        var counter = 1;

                        while (wishListItem.List.Items.Any(x => x.Name == itemName))
                        {
                            itemName = $"{baseName}{counter}";
                            counter++;
                        }

                        wishListItem.Name = itemName;
                        await _wishListItemsService.Update(wishListItem.Id, wishListItem.Name, wishListItem.Description, wishListItem.Link, wishListItem.Photo, wishListItem.Video, wishListItem.PerformerName, wishListItem.CurrentState);
                        await BotActions.ShowList(message, token, _localization, wishListItem.Name);
                    }
                    break;
                case UserState.SettingDescription:
                    var description = message.Text;
                    if (description == null)
                    {
                        await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.DescriptionCantBeEmpty), cancellationToken: token);
                        break;
                    }
                    if (ObjectToUpdate is WishListItemEntity item)
                    {
                        item.Description = description;
                        await _wishListItemsService.Update(item.Id, item.Name, item.Description, item.Link, item.Photo, item.Video, item.PerformerName, item.CurrentState);
                        await BotActions.ShowList(message, token, _localization, item.List.Name);
                    }
                    break;
                case UserState.SettingLink:
                    if (!Uri.IsWellFormedUriString(message.Text, UriKind.Absolute))
                    {
                        await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.InvalidLink), cancellationToken: token);
                        break;
                    }
                    var link = message.Text;
                    if (ObjectToUpdate is WishListItemEntity itm)
                    {
                        itm.Link = link;
                        await _wishListItemsService.Update(itm.Id, itm.Name, itm.Description, itm.Link, itm.Photo, itm.Video, itm.PerformerName, itm.CurrentState);
                        await BotActions.ShowList(message, token, _localization, itm.List.Name);
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
                        if (ObjectToUpdate is WishListItemEntity wish)
                        {
                            if (wish.Video != null)
                            {
                                wish.Video = null;
                                break;
                            }
                            wish.Photo = new PhotoSizeEntity() 
                            {
                                FileId = message.Photo[0].FileId,
                                Width = message.Photo[0].Width,
                                Height = message.Photo[0].Height
                            };
                            await _wishListItemsService.Update(wish.Id, wish.Name, wish.Description, wish.Link, wish.Photo, wish.Video, wish.PerformerName, wish.CurrentState);
                            await BotActions.ShowList(message, token, _localization, wish.List.Name);
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
                        if (ObjectToUpdate is WishListItemEntity wish)
                        {
                            if (wish.Photo != null)
                            {
                                Console.WriteLine(2);

                                wish.Photo = null;
                                break;
                            }
                            wish.Video = new VideoEntity()
                            {
                                FileId = message.Video.FileId,
                                Duration = message.Video.Duration,
                                Width = message.Video.Width,
                                Height = message.Video.Height
                            };
                            Console.WriteLine(wish.Video.Duration);

                            await _wishListItemsService.Update(wish.Id, wish.Name, wish.Description, wish.Link, wish.Photo, wish.Video, wish.PerformerName, wish.CurrentState);
                            await BotActions.ShowList(message, token, _localization, wish.List.Name);
                        }
                        break;
                    }

                    await BotActions.ShowList(message,  token,  _localization, ((WishListItemEntity)ObjectToUpdate).List.Name);

                    break;
                case UserState.Free:
                    
                    break;
            }
            RemoveState(userId);
        }
   */
    }
}
