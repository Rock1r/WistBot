using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Managers;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class CancelCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly UserStateManager _userStateManager;

        public string Command => BotCallbacks.Cancel;

        public CancelCallbackAction(ITelegramBotClient bot, LocalizationService localizationService, UserStateManager userStateManager)
        {
            _bot = bot;
            _localization = localizationService;
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
                var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
                var chatId = message.Chat.Id;
                var sender = callback.From ?? throw new ArgumentNullException(nameof(callback.From));
                var context = UserContextManager.GetContext(sender.Id);
                _userStateManager.RemoveState(sender.Id);
                context.MessagesToDelete.Add(message);
                await UserContextManager.DeleteMessages(_bot, sender.Id, chatId, context, token);
                await _bot.AnswerCallbackQuery(callback.Id, await _localization.Get(LocalizationKeys.Cancelled, sender.Id), cancellationToken: token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error CancelCallbackAction");
            }
        }
    }
}
