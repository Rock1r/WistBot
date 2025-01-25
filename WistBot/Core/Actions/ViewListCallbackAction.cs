using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Exceptions;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class ViewListCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly WishListsService _wishListsService;
        private readonly LocalizationService _localization;

        public string Command => BotCallbacks.List;

        public ViewListCallbackAction(ITelegramBotClient bot, WishListsService wishListsService, LocalizationService localizationService)
        {
            _bot = bot;
            _wishListsService = wishListsService;
            _localization = localizationService;
        }

        public Task ExecuteMessage(Message message, CancellationToken token)
        {
           return Task.CompletedTask;
        }
        
        public async Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            try
            {
                var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
                var chatId = message.Chat.Id;
                var user = callback.From ?? throw new ArgumentNullException(nameof(callback.From));
                var culture = await _localization.GetLanguage(callback.From.Id);
                var list = await _wishListsService.GetByName(user.Id, message.Text ?? throw new ArgumentNullException(nameof(message.Text)));
                var messageToSend = await _localization.Get(LocalizationKeys.ViewListMessage, user.Id, list.Name);
                if (!list.Items.Any())
                {
                    messageToSend = await _localization.Get(LocalizationKeys.ListIsEmpty, culture, user.Username ?? user.FirstName, list.Name);
                }
                await _bot.AnswerCallbackQuery(callback.Id, messageToSend, cancellationToken: token);

                await WishListsService.ViewList(_bot, chatId, user.Id, list, _localization, token);
                Log.Information("User {UserId} viewed list {ListName}", user.Id, list.Name);
            }
            catch(ListNotFoundException)
            {
                await _bot.AnswerCallbackQuery(callback.Id, await _localization.Get(LocalizationKeys.ListNotFound, callback.From.Id), cancellationToken: token);
                Log.Information("List not found");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error ViewListCallbackAction");
            }
        }
    }
}
