using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;
using WistBot.Res;
using WistBot.Managers;
using Serilog;

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
                var context = UserContextManager.GetContext(userId) ?? throw new ArgumentNullException();
                if (string.IsNullOrWhiteSpace(description))
                {
                    var warning = await bot.SendMessage(message.Chat.Id, localization.Get(LocalizationKeys.DescriptionCantBeEmpty), cancellationToken: token);
                    context.MessagesToDelete.Add(message);
                    context.MessagesToDelete.Add(warning);
                    return false;
                }

                _wishListItem.Description = description;
                await wishListItemsService.Update(_wishListItem);
                var text = MessageBuilder.BuildItemMessage(_wishListItem);
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
                context.MessagesToDelete.Add(message);
                await UserContextManager.DeleteMessages(bot, userId, message.Chat.Id, context, token);
                Log.Information($"User {userId} set item description to {_wishListItem.Description}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while setting item description");
                return false; // Ensure a return value in case of exception
            }
            
        }
    }

}
