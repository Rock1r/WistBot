using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;

namespace WistBot.Core.UserStates
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
            try
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
                await wishListsService.ViewList(bot, message.Chat.Id, userId, await wishListsService.GetById(_wishList.Id), localization, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error AddingNewItemState: {ex.Message}");
            }
        }
    }
}
