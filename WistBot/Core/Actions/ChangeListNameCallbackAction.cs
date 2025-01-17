using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Core.UserStates;
using WistBot.Exceptions;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class ChangeListNameCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly WishListsService _wishListsService;
        private readonly UserStateManager _userStateManager;

        public string Command => BotCallbacks.ChangeListName;

        public ChangeListNameCallbackAction(ITelegramBotClient bot, LocalizationService localizationService, WishListsService wishListsService, UserStateManager userStateManager)
        {
            _bot = bot;
            _localization = localizationService;
            _wishListsService = wishListsService;
            _userStateManager = userStateManager;
        }

        public Task ExecuteMessage(Message message, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            try
            {
                var userId = callback.From.Id;
                var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
                var listName = message.Text ?? throw new ArgumentNullException(nameof(message.Text));
                var list = await _wishListsService.GetByName(userId, listName);
                _userStateManager.SetState(userId, new SettingListNameState(list));
                await _bot.SendMessage(message.Chat.Id, await _localization.Get(LocalizationKeys.SetListName, userId), cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ChangeListNameCallbackAction: {ex.Message}");
            }
        }
    }
}
