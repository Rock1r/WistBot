using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Data.Models;
using WistBot.Services;
using WistBot.States;

namespace WistBot.UserStates
{
    public class ViewingListState : IUserStateHandler
    {
        private readonly WishListEntity _wishList;
        private readonly UserStateManager _userStateManager;

        public ViewingListState(WishListEntity wishList, UserStateManager userStateManager)
        {
            _wishList = wishList ?? throw new ArgumentNullException(nameof(wishList));
            _userStateManager = userStateManager ?? throw new ArgumentNullException(nameof(userStateManager));
        }

        public async Task HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, WishListItemsService wishListItemsService)
        {
            if (message.Text == Button.AddItem)
            {
                await BotActions.AddListItemAction(message, token, localization, _wishList);
            }
        }
    }
}