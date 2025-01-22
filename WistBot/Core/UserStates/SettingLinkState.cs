using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;
using WistBot.Res;
using WistBot.Managers;

namespace WistBot.Core.UserStates
{
    public class SettingLinkState : IUserStateHandler
    {
        private readonly ItemEntity _wishListItem;

        public SettingLinkState(ItemEntity wishListItem)
        {
            _wishListItem = wishListItem ?? throw new ArgumentNullException(nameof(wishListItem));
        }

        public async Task<bool> HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, ItemsService wishListItemsService)
        {
            try
            {
                var link = message.Text;

                if (!Uri.IsWellFormedUriString(link, UriKind.Absolute))
                {
                    var warning = await bot.SendMessage(message.Chat.Id, await localization.Get(LocalizationKeys.InvalidLink, userId), cancellationToken: token);
                    var usercontext = UserContextManager.GetContext(userId);
                    usercontext.MessagesToDelete.Add(message);
                    usercontext.MessagesToDelete.Add(warning);
                    return false;
                }

                _wishListItem.Link = link;

                await wishListItemsService.Update(_wishListItem);
                var context = UserContextManager.GetContext(userId) ?? throw new ArgumentNullException();
                var mes = context.MessageToEdit ?? throw new ArgumentNullException();
                var newText = MessageBuilder.BuildItemMessage(_wishListItem);
                var replyMarkup = await ItemsService.BuildItemMarkup(userId, localization);
                if (!string.IsNullOrEmpty(_wishListItem.Media))
                {
                    await bot.EditMessageCaption(mes.Chat.Id, mes.Id, newText, replyMarkup: replyMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, cancellationToken: token);
                }
                else
                {
                    await bot.EditMessageText(mes.Chat.Id, mes.Id, newText, replyMarkup: replyMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, cancellationToken: token);
                }
                var messagesToDelete = new List<int>();
                messagesToDelete.Add(message.MessageId);
                foreach (var msg in context.MessagesToDelete)
                {
                    messagesToDelete.Add(msg.MessageId);
                }
                await bot.DeleteMessages(message.Chat.Id, messagesToDelete, cancellationToken: token);
                UserContextManager.ClearContext(userId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error SettingLinkState: {ex.Message}");
                return false; // Ensure a return value in case of exception
            }
        }
    }
}
