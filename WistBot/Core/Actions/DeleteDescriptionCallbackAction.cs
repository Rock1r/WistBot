using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Exceptions;
using WistBot.Managers;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class DeleteDescriptionCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly ItemsService _wishListItemsService;
        private readonly LocalizationService _localization;

        public string Command => BotCallbacks.DeleteDescription;

        public DeleteDescriptionCallbackAction(ITelegramBotClient bot, ItemsService wishListItemsService, LocalizationService localizationService)
        {
            _bot = bot;
            _wishListItemsService = wishListItemsService;
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
                if (callback == null)
                {
                    return;
                }
                var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
                var chatId = message.Chat.Id;
                var userId = callback.From.Id;
                var context = UserContextManager.GetContext(userId);
                var item = context.ItemToEdit ?? throw new ArgumentNullException(nameof(context.ItemToEdit));
                item.Description = string.Empty;
                await _wishListItemsService.Update(item);
                List<int> messagesToDelete = new();
                messagesToDelete.Add(message.MessageId);
                foreach (var msg in context.MessagesToDelete)
                {
                    messagesToDelete.Add(msg.MessageId);
                }
                var messageToEdit = context.MessageToEdit ?? throw new ArgumentNullException(nameof(context.MessageToEdit));
                await _bot.DeleteMessages(chatId, messagesToDelete, cancellationToken: token);
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
                UserContextManager.ClearContext(userId);
            }
            catch (ItemNotFoundException ex)
            {
                await _bot.AnswerCallbackQuery(callback.Id, ex.Message, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error DeleteDescriptionCallbackAction: {ex.Message}");
            }
        }
    }
}
