using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class DeleteItemCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly ItemsService _itemsService;

        public string Command => BotCallbacks.DeleteItem;

        public DeleteItemCallbackAction(ITelegramBotClient bot, LocalizationService localizationService, ItemsService itemsService)
        {
            _bot = bot;
            _localization = localizationService;
            _itemsService = itemsService;
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
                var text = message.Text ?? message.Caption ?? throw new ArgumentNullException(nameof(message.Text));
                text = text.Split("\n")[0];
                await _itemsService.Delete(userId, text);
                await _bot.AnswerCallbackQuery(callback.Id, _localization.Get(LocalizationKeys.ItemDeleted), cancellationToken: token);
                await _bot.DeleteMessage(message.Chat.Id, message.MessageId, cancellationToken: token);
                Log.Information($"Item {text} deleted by user {userId}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error DeleteItemCallbackAction");
            }
        }
    }
}
