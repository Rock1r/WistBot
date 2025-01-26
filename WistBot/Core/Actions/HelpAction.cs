using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class HelpAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;

        public string Command => BotCommands.Help;

        public HelpAction(ITelegramBotClient bot, LocalizationService localizationService)
        {
            _bot = bot;
            _localization = localizationService;
        }

        public async Task ExecuteMessage(Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;
            try
            {
                var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                var text = await _localization.Get(LocalizationKeys.HelpMessage, user.Id, BotCommands.Language, BotCommands.Lists, BotCommands.Feedback, BotCommands.MyPresents, await _localization.Get(InlineButton.WantToPresent, user.Id));
                await _bot.SendMessage(chatId, text.Replace("/n", Environment.NewLine), replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
                Log.Information("HelpAction executed for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error HelpAction");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
