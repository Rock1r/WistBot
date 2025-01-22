using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Core.UserStates;
using WistBot.Services;

namespace WistBot.Managers
{
    public class UserStateManager
    {
        private readonly Dictionary<long, IUserStateHandler> _userStates = new();

        public void SetState(long userId, IUserStateHandler stateHandler)
        {
            _userStates[userId] = stateHandler;
        }

        public async Task HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, ItemsService wishListItemsService)
        {
            if (_userStates.TryGetValue(userId, out var stateHandler))
            {
                if (await stateHandler.HandleStateAsync(userId, message, bot, token, localization, wishListsService, wishListItemsService))
                {
                    _userStates.Remove(userId);
                }
            }
        }

        public void RemoveState(long userId)
        {
            _userStates.Remove(userId);
        }

        public bool UserHasState(long userId)
        {
            return _userStates.ContainsKey(userId);
        }

        public void ClearStates()
        {
            _userStates.Clear();
        }
    }
}
