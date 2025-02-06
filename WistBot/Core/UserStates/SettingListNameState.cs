using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Core.Actions;
using WistBot.Data.Models;
using WistBot.Managers;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.UserStates
{
    public class SettingListNameState : IUserStateHandler
    {
        private readonly WishListEntity _wishList;
        private readonly UsersService _usersService;

        public SettingListNameState(WishListEntity wishList, UsersService usersService)
        {
            _wishList = wishList;
            _usersService = usersService;
        }

        public async Task<bool> HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, ItemsService wishListItemsService)
        {
            try
            {
                var newListName = message.Text;

                if (string.IsNullOrWhiteSpace(newListName))
                {
                    var warning = await bot.SendMessage(message.Chat.Id, localization.Get(LocalizationKeys.ListNameCantBeEmpty), cancellationToken: token);
                    var context = UserContextManager.GetContext(userId);
                    context.MessagesToDelete.Add(message);
                    context.MessagesToDelete.Add(warning);
                    return false;
                }


                var counter = 0;
                var lists = await wishListsService.GetByOwnerId(userId);
                while (lists.Any(x => x.Name == newListName))
                {
                    counter++;
                    newListName = $"{message.Text} ({counter})";
                }

                if (_wishList != null)
                {
                    var context = UserContextManager.GetContext(userId);
                    if(_wishList.Name == newListName)
                    {
                        context.MessagesToDelete.Add(message);
                        await UserContextManager.DeleteMessages(bot, userId, message.Chat.Id, context, token);
                        return true;
                    }

                    await wishListsService.Update(_wishList.Id, newListName, _wishList.IsPublic);
                    var inlineReply = await WishListsService.GetListMarkup(_wishList, localization);
                    await bot.EditMessageText(context.MessageToEdit.Chat.Id, context.MessageToEdit.MessageId, newListName, replyMarkup: inlineReply, cancellationToken: token);
                    context.MessagesToDelete.Add(message);
                    await UserContextManager.DeleteMessages(bot, userId, message.Chat.Id, context, token);

                    Log.Information($"User {userId} updated list {_wishList.Id} name to {newListName}");
                }
                else
                {
                    var user = await _usersService.GetById(userId);
                    if (user.WishLists.Count < user.MaxListsCount)
                    {
                        await wishListsService.Add(newListName, userId);
                        await new ListsAction(bot, wishListsService, localization).ExecuteMessage(message, token);
                    }
                    else
                    {
                        await bot.SendMessage(message.Chat.Id, await localization.Get(LocalizationKeys.MaxListsCountReached, userId), cancellationToken: token);
                    }
                    Log.Information($"User {userId} created new list {newListName}");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error SettingListNameState");
                return false;
            }
        }
    }
}
