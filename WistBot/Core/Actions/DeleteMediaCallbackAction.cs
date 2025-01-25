using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Exceptions;
using WistBot.Managers;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class DeleteMediaCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly ItemsService _wishListItemsService;
        private readonly LocalizationService _localization;

        public string Command => BotCallbacks.DeleteMedia;

        public DeleteMediaCallbackAction(ITelegramBotClient bot, ItemsService wishListItemsService, LocalizationService localizationService)
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
                item.Media = string.Empty;
                await _wishListItemsService.Update(item);
                context.MessagesToDelete.Add(message);
                context.MessagesToDelete.Add(context.MessageToEdit);
                var inlineReply = await ItemsService.BuildItemMarkup(userId, _localization);
                var newText = MessageBuilder.BuildItemMessage(item);
                await _bot.SendMessage(chatId, newText, replyMarkup: inlineReply, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, cancellationToken: token);
                await UserContextManager.DeleteMessages(_bot, userId, chatId, context, token);
            }
            catch (ItemNotFoundException ex)
            {
                await _bot.AnswerCallbackQuery(callback.Id, ex.Message, cancellationToken: token);
                Log.Error(ex, "Error DeleteMediaCallbackAction, item not found");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error DeleteMediaCallbackAction");
            }
        }
    }
}
