using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class LanguageAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly UsersService _usersService;
        private readonly LocalizationService _localization;

        public string Command => BotCommands.Language;

        public LanguageAction(ITelegramBotClient bot, UsersService usersService, LocalizationService localizationService)
        {
            _bot = bot;
            _usersService = usersService;
            _localization = localizationService;
        }

        public async Task ExecuteMessage(Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;
            try
            {
                var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                var keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(Button.Ukrainian), new KeyboardButton(Button.English));
                await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.ChooseLanguage, user.Id), replyMarkup: keyboard, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error LanguageAction: {ex.Message}");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
