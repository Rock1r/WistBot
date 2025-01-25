using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Data.Models;
using WistBot.Managers;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.UserStates
{
    public class SettingItemNameState : IUserStateHandler
    {
        private readonly ItemEntity _wishListItem;

        public SettingItemNameState(ItemEntity wishListItem)
        {
            _wishListItem = wishListItem ?? throw new ArgumentNullException(nameof(wishListItem));
        }

        public async Task<bool> HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, ItemsService wishListItemsService)
        {
            try
            {

                var itemName = message.Text;
                
                var context = UserContextManager.GetContext(userId) ?? throw new ArgumentNullException();

                if (string.IsNullOrWhiteSpace(itemName))
                {
                    var warning = await bot.SendMessage(message.Chat.Id, localization.Get(LocalizationKeys.ItemNameCantBeEmpty), cancellationToken: token);
                    context.MessagesToDelete.Add(message);
                    context.MessagesToDelete.Add(warning);
                    return false;
                }

                var baseName = itemName;
                var counter = 1;

                var list = await wishListsService.GetById(_wishListItem.ListId);

                while (list.Items.Any(x => x.Name == itemName))
                {
                    itemName = $"{baseName} ({counter})";
                    counter++;
                }

                _wishListItem.Name = itemName;

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

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error SettingItemNameState: {ex.Message}");
                return false;
            }
        }
    }
}
