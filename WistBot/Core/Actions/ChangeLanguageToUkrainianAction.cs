using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class ChangeLanguageToUkrainianAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly UsersService _usersService;
        private readonly LocalizationService _localization;

        public string Command => Button.Ukrainian;

        public ChangeLanguageToUkrainianAction(ITelegramBotClient bot, UsersService usersService, LocalizationService localizationService)
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
                await _usersService.SetLanguage(user.Id, LanguageCodes.Ukrainian);

                await _bot.SendMessage(message.Chat.Id, await _localization.Get(LocalizationKeys.LanguageChanged, user.Id), replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ChangeLanguageToUkrainianAction: {ex.Message}");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
