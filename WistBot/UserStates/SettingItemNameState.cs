using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;

namespace WistBot.States
{
    public class SettingItemNameState : IUserStateHandler
    {
        private readonly WishListItemEntity _wishListItem;

        public SettingItemNameState(WishListItemEntity wishListItem)
        {
            _wishListItem = wishListItem ?? throw new ArgumentNullException(nameof(wishListItem));
        }

        public async Task HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, WishListItemsService wishListItemsService)
        {
            var itemName = message.Text;

            if (string.IsNullOrWhiteSpace(itemName))
            {
                await bot.SendMessage(message.Chat.Id, localization.Get(LocalizationKeys.NameCantBeEmpty), cancellationToken: token);
                return;
            }

            var baseName = itemName;
            var counter = 1;

            var list = await wishListsService.GetById(_wishListItem.ListId);

            while (list.Items.Any(x => x.Name == itemName))
            {
                itemName = $"{baseName}{counter}";
                counter++;
            }

            _wishListItem.Name = itemName;

            await wishListItemsService.Update(_wishListItem);

            await BotActions.ShowList(message, token, localization, list);
        }
    }
}
