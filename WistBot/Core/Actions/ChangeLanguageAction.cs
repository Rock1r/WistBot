using System.Globalization;
using System.Resources;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class ChangeLanguageAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly ResourceManager _resourceManager;

        public string Command => KButton.ChangeLanguage;

        public ChangeLanguageAction(ITelegramBotClient bot, LocalizationService localizationService, ResourceManager resourceManager)
        {
            _bot = bot;
            _localization = localizationService;
            _resourceManager = resourceManager;
        }

        public async Task ExecuteMessage(Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;
            try
            {
                var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                var buttonText = message.Text;
                string? selectedLanguage = null;
                foreach (var culture in _localization.AvailableLanguages)
                {
                    var localizedButton = _resourceManager.GetString(KButton.ChangeLanguage, new CultureInfo(culture));
                    if (localizedButton == buttonText)
                    {
                        selectedLanguage = culture;
                        break;
                    }
                }
                var currentLanguage = await _localization.GetLanguage(user.Id);
                if (currentLanguage.Name == selectedLanguage)
                {
                    await _bot.SendMessage(
                        chatId,
                        await _localization.Get(LocalizationKeys.LanguageAlreadySet, user.Id),
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: token
                    );
                }
                else
                {
                    await _localization.SetLanguage(user.Id, selectedLanguage?? throw new Exception());
                    await _bot.SendMessage(
                        chatId,
                        await _localization.Get(LocalizationKeys.LanguageChanged, user.Id),
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: token
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ChangeLanguageToEnglistAction: {ex.Message}");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
