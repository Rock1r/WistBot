using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Exceptions;
using WistBot.Managers;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class DeleteLinkCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly ItemsService _wishListItemsService;
        private readonly LocalizationService _localization;
        private readonly UserStateManager _stateManager;

        public string Command => BotCallbacks.DeleteLink;

        public DeleteLinkCallbackAction(ITelegramBotClient bot, ItemsService wishListItemsService, LocalizationService localizationService, UserStateManager stateManager)
        {
            _bot = bot;
            _wishListItemsService = wishListItemsService;
            _localization = localizationService;
            _stateManager = stateManager;
        }

        public Task ExecuteMessage(Message message, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            try
            {
                if (callback == null)
                {
                    return;
                }
                var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
                var chatId = message.Chat.Id;
                var userId = callback.From.Id;
                var context = UserContextManager.GetContext(userId);
                var item = context.ItemToEdit ?? throw new ArgumentNullException(nameof(context.ItemToEdit));
                item.Link = string.Empty;
                await _wishListItemsService.Update(item);

                context.MessagesToDelete.Add(message);
                var messageToEdit = context.MessageToEdit ?? throw new ArgumentNullException(nameof(context.MessageToEdit));
                var inlineReply = await ItemsService.BuildItemMarkup(userId, _localization);
                var newText = MessageBuilder.BuildItemMessage(item);
                if (!string.IsNullOrEmpty(item.Media))
                {
                    await _bot.EditMessageCaption(chatId, messageToEdit.MessageId, newText, Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineReply, cancellationToken: token);
                }
                else
                {
                    await _bot.EditMessageText(chatId, messageToEdit.MessageId, newText, replyMarkup: inlineReply, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, cancellationToken: token);
                }
                await UserContextManager.DeleteMessages(_bot, userId, chatId, context, token);
                _stateManager.RemoveState(userId);
            }
            catch (ItemNotFoundException ex)
            {
                await _bot.AnswerCallbackQuery(callback.Id, ex.Message, cancellationToken: token);
                Log.Error(ex, "Error DeleteLinkCallbackAction, item not found");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error DeleteLinkCallbackAction");
            }
        }
    }
}
