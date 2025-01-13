using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Services;

namespace WistBot.States
{
    public interface IUserStateHandler
    {
        public Task HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, WishListItemsService wishListItemsService);
    }

}
