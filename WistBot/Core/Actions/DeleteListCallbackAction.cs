using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class DeleteListCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly WishListsService _wishListsService;

        public string Command => BotCallbacks.DeleteList;

        public DeleteListCallbackAction(ITelegramBotClient bot, LocalizationService localizationService, WishListsService wishListsService)
        {
            _bot = bot;
            _localization = localizationService;
            _wishListsService = wishListsService;
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
                var listName = message.Text ?? throw new ArgumentNullException(nameof(message.Text));
                await _wishListsService.Delete(userId, listName);
                await _bot.AnswerCallbackQuery(callback.Id, _localization.Get(LocalizationKeys.ListDeleted), cancellationToken: token);
                await _bot.DeleteMessage(message.Chat.Id, message.MessageId, cancellationToken: token);
                Log.Information("List {ListName} deleted by {UserId}", listName, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error DeleteListCallbackAction");
            }
        }
    }
}
