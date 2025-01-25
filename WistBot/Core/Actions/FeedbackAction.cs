using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Core.UserStates;
using WistBot.Managers;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class FeedbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly UserStateManager _userStateManager;
        private readonly UsersService _usersService;

        public string Command => BotCommands.Feedback;

        public FeedbackAction(ITelegramBotClient bot, UserStateManager userStateManager, LocalizationService localizationService, UsersService usersService)
        {
            _bot = bot;
            _userStateManager = userStateManager;
            _localization = localizationService;
            _usersService = usersService;
        }

        public async Task ExecuteMessage(Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;
            try
            {
                var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                _userStateManager.SetState(user.Id, new SendingFeedbackState(_usersService));
                await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.FeedbackMessage, user.Id), replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
                Log.Information("FeedbackAction: User {UserId} started sending feedback", user.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error FeedbackAction");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
