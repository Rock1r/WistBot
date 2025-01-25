using Serilog;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Res;
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
                var keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(await _localization.Get(KButton.ChangeLanguage, new CultureInfo(LanguageCodes.English))), new KeyboardButton(await _localization.Get(KButton.ChangeLanguage, new CultureInfo(LanguageCodes.Ukrainian))));
                await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.ChooseLanguage, user.Id), replyMarkup: keyboard, cancellationToken: token);
                Log.Information("LanguageAction: User {UserId} started changing language", user.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error LanguageAction");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
