using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Core.UserStates;
using WistBot.Exceptions;
using WistBot.Managers;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class ChangeItemNameCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly UserStateManager _userStateManager;
        private readonly ItemsService _wishListItemsService;

        public string Command => BotCallbacks.ChangeItemName;

        public ChangeItemNameCallbackAction(ITelegramBotClient bot, LocalizationService localizationService, ItemsService wishListItemsService, UserStateManager userStateManager)
        {
            _bot = bot;
            _localization = localizationService;
            _wishListItemsService = wishListItemsService;
            _userStateManager = userStateManager;
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
                var text = message.Text ?? message.Caption ?? throw new ArgumentNullException(nameof(message.Text));
                text = text.Split("\n")[0];
                var item = await _wishListItemsService.GetByName(userId, text);
                _userStateManager.SetState(userId, new SettingItemNameState(item));
                var keyboard = new ReplyKeyboardMarkup(true).AddButton(await _localization.Get(LocalizationKeys.DefaultItemNaming, userId));
                var mes = await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.SetItemName, userId), replyMarkup: keyboard, cancellationToken: token);
                UserContextManager.SetContext(userId, new UserContext(mes) { MessageToEdit = message});
            }
            catch (ItemNotFoundException ex)
            {
                await _bot.AnswerCallbackQuery(callback.Id, ex.Message, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ChangeItemNameCallbackAction: {ex.Message}");
            }
        }
    }
}
