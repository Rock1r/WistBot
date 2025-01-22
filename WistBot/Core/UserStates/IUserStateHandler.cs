using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Services;

namespace WistBot.Core.UserStates
{
    public interface IUserStateHandler
    {
        public Task<bool> HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, ItemsService wishListItemsService);
    }

}
