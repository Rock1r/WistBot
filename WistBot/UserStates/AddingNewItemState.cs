using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;

namespace WistBot.States
{
    public class AddingNewItemState : IUserStateHandler
    {
        private readonly WishListEntity _wishList;

        public AddingNewItemState(WishListEntity wishList)
        {
            _wishList = wishList ?? throw new ArgumentNullException(nameof(wishList));
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

            while (_wishList.Items.Any(x => x.Name == itemName))
            {
                itemName = $"{baseName}{counter}";
                counter++;
            }

            await wishListItemsService.Add(new WishListItemEntity { Name = itemName, ListId = _wishList.Id });
            await BotActions.ShowList(message, token, localization, await wishListsService.GetById(_wishList.Id));
        }
    }
}
