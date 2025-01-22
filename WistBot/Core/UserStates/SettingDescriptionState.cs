using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;
using WistBot.Res;
using WistBot.Managers;

namespace WistBot.Core.UserStates
{
    public class SettingDescriptionState : IUserStateHandler
    {
        private readonly ItemEntity _wishListItem;

        public SettingDescriptionState(ItemEntity wishListItem)
        {
            _wishListItem = wishListItem ?? throw new ArgumentNullException(nameof(wishListItem));
        }

        public async Task<bool> HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, ItemsService wishListItemsService)
        {
            try
            {
                var description = message.Text;
                if (string.IsNullOrWhiteSpace(description))
                {
                    var warning = await bot.SendMessage(message.Chat.Id, localization.Get(LocalizationKeys.DescriptionCantBeEmpty), cancellationToken: token);
                    var usercontext = UserContextManager.GetContext(userId);
                    usercontext.MessagesToDelete.Add(message);
                    usercontext.MessagesToDelete.Add(warning);
                    return false;
                }

                _wishListItem.Description = description;
                await wishListItemsService.Update(_wishListItem);
                var text = MessageBuilder.BuildItemMessage(_wishListItem);
                var context = UserContextManager.GetContext(userId) ?? throw new ArgumentNullException();
                var mes = context.MessageToEdit ?? throw new ArgumentNullException();
                var replyMarkup = await ItemsService.BuildItemMarkup(userId, localization);
                if (!string.IsNullOrEmpty(_wishListItem.Media))
                {
                    await bot.EditMessageCaption(mes.Chat.Id, mes.Id, text, replyMarkup: replyMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, cancellationToken: token);
                }
                else
                {
                    await bot.EditMessageText(mes.Chat.Id, mes.Id, text, replyMarkup: replyMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, cancellationToken: token);
                }
                var messagesToDelete = new List<int>();
                messagesToDelete.Add(message.MessageId);
                foreach (var msg in context.MessagesToDelete)
                {
                    messagesToDelete.Add(msg.MessageId);
                }
                await bot.DeleteMessages(message.Chat.Id, messagesToDelete.ToArray(), cancellationToken: token);
                UserContextManager.ClearContext(userId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error SettingDescriptionState: {ex.Message}");
                return false; // Ensure a return value in case of exception
            }
            
        }
    }

}
