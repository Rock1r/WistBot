using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Core.Actions;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Services;
using WistBot.Res;
using WistBot.Managers;

namespace WistBot.Core.UserStates
{
    public class SettingListNameState : IUserStateHandler
    {
        private readonly WishListEntity _wishList;

        public SettingListNameState(WishListEntity wishList)
        {
            _wishList = wishList;
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
                    await wishListsService.Update(_wishList.Id, newListName, _wishList.IsPublic);
                    var context = UserContextManager.GetContext(userId);
                    var inlineReply = await WishListsService.GetListMarkup(_wishList, localization);
                    await bot.EditMessageText(context.MessageToEdit.Chat.Id, context.MessageToEdit.MessageId, newListName, replyMarkup: inlineReply, cancellationToken: token);
                    var messagesToDelete = new List<int>();
                    messagesToDelete.Add(message.MessageId);
                    foreach (var msg in context.MessagesToDelete)
                    {
                        messagesToDelete.Add(msg.MessageId);
                    }
                    await bot.DeleteMessages(message.Chat.Id, messagesToDelete, cancellationToken: token);
                    UserContextManager.ClearContext(userId);
                }
                else
                {
                    await wishListsService.Add(newListName, false, userId);
                    await new ListsAction(bot, wishListsService, localization).ExecuteMessage(message, token);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error SettingListNameState: {ex.Message}");
                return false;
            }
        }
    }
}
