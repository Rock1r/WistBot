using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;
using WistBot.Res;
using WistBot.Managers;
using Serilog;

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

                var context = UserContextManager.GetContext(userId) ?? throw new ArgumentNullException();
                if (!Uri.IsWellFormedUriString(link, UriKind.Absolute))
                {
                    var warning = await bot.SendMessage(message.Chat.Id, await localization.Get(LocalizationKeys.InvalidLink, userId), cancellationToken: token);
                    context.MessagesToDelete.Add(message);
                    context.MessagesToDelete.Add(warning);
                    return false;
                }

                _wishListItem.Link = link;

                await wishListItemsService.Update(_wishListItem);
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
                context.MessagesToDelete.Add(message);
                await UserContextManager.DeleteMessages(bot, userId, message.Chat.Id, context, token);
                Log.Information($"User {userId} set link to item {_wishListItem.Id}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while setting link");
                return false; // Ensure a return value in case of exception
            }
        }
    }
}
