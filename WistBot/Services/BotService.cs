using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Managers;

namespace WistBot.Services
{
    public class BotService
    {
        private readonly ITelegramBotClient _client;
        private readonly WishListsService _wishListsService;
        private readonly ItemsService _wishListItemsService;
        private readonly UserStateManager _userStateManager;
        private readonly LocalizationService _localization;
        private readonly ActionService _actionService;

        public BotService(ITelegramBotClient bot,
            WishListsService wishListService,
            ItemsService wishListItemsService,
            UserStateManager userStateManager,
            LocalizationService localization,
            ActionService actionService
            )
        {
            _client = bot;
            _wishListsService = wishListService;
            _wishListItemsService = wishListItemsService;
            _userStateManager = userStateManager;
            _localization = localization;
            _actionService = actionService;
            bot.DropPendingUpdates();
        }

        public void StartReceiving()
        {
            _client.StartReceiving(HandleUpdate, HandleError);
        }

        private async Task HandleUpdate(ITelegramBotClient _client, Update update, CancellationToken token)
        {
            if (update.Message is { } message)
            {
                Log.Information("Received message from {Username} ({UserId}): {Message}", message.From.Username, message.From.Id, message.Text);
                try
                {
                    var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                    if (_userStateManager.UserHasState(user.Id))
                    {
                        await _userStateManager.HandleStateAsync(user.Id, message, _client, token, _localization, _wishListsService, _wishListItemsService);
                        return;
                    }
                    await _actionService.ExecuteMessage(message.Text ?? string.Empty, message, token);
                }
                catch (KeyNotFoundException)
                {
                    Log.Warning("Unknown action {Username} ({UserId}): {Message}", message.From.Username, message.From.Id, message.Text);
                    await _client.DeleteMessage(message.Chat.Id, message.MessageId, cancellationToken: token);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while processing message from {Username} ({UserId}): {Message}", message.From.Username, message.From.Id, message.Text);
                }
            }
            else if (update.CallbackQuery is { } callback)
            {
                Log.Information("Received callback from {Username} ({UserId}): {Data}", callback.From.Username, callback.From.Id, callback.Data);
                try
                {
                    await _actionService.ExecuteCallback(callback.Data ?? string.Empty, callback, token);
                }
                catch (KeyNotFoundException)
                {
                    Log.Warning("Unknown action");
                    await _client.AnswerCallbackQuery(callback.Id, "Unknown action", cancellationToken: token);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while processing callback from {Username} ({UserId}): {Message}", callback.From.Username, callback.From.Id, callback.Data);
                }
            }
        }

        private Task HandleError(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Log.Error(exception, "Error while receiving updates");
            return Task.CompletedTask;
        }
    }
}