using Telegram.Bot;
using Telegram.Bot.Types;
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
                var listName = message.Text;
                await _wishListsService.Delete(listName ?? throw new ArgumentNullException(nameof(listName)));
                await _bot.AnswerCallbackQuery(callback.Id, _localization.Get(LocalizationKeys.ListDeleted), cancellationToken: token);
                await _bot.DeleteMessage(message.Chat.Id, message.MessageId, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error DeleteListCallbackAction: {ex.Message}");
            }
        }
    }
}
