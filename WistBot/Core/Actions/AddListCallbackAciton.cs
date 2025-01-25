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
    public class AddListCallbackAciton : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly UserStateManager _userStateManager;
        private readonly LocalizationService _localization;
        private readonly UsersService _usersService;

        public string Command => BotCallbacks.AddList;

        public AddListCallbackAciton(ITelegramBotClient bot, UserStateManager userStateManager, LocalizationService localizationService, UsersService usersService)
        {
            _bot = bot;
            _userStateManager = userStateManager;
            _localization = localizationService;
            _usersService = usersService;
        }

        public Task ExecuteMessage(Message message, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            try
            {
                var chatId = callback.Message?.Chat.Id;
                var user = callback.From ?? throw new ArgumentNullException(nameof(callback.From));
                _userStateManager.SetState(user.Id, new SettingListNameState(null!, _usersService));
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.Cancel, user.Id), BotCallbacks.Cancel)
                    }
                });
                var mes = await _bot.SendMessage(chatId ?? throw new Exception(), await _localization.Get(LocalizationKeys.SetListName, user.Id), replyMarkup: keyboard, cancellationToken: token);
                UserContextManager.SetContext(user.Id, new UserContext(mes));
            }
            catch (Exception ex)
            {
                Log.Error($"Error AddListCallbackAction: {ex.Message}");
            }
        }
    }
}
