using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Core.UserStates;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class AddListAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly UserStateManager _userStateManager;
        private readonly LocalizationService _localization;

        public string Command => KButton.AddList;

        public AddListAction(ITelegramBotClient bot, UserStateManager userStateManager, LocalizationService localizationService)
        {
            _bot = bot;
            _userStateManager = userStateManager;
            _localization = localizationService;
        }

        public async Task ExecuteMessage(Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;
            try
            {
                var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                _userStateManager.SetState(user.Id, new SettingListNameState(null!));
                await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.SetListName, user.Id), replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error AddListAction: {ex.Message}");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
