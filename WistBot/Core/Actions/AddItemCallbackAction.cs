using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Core.UserStates;
using WistBot.Managers;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class AddItemCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly WishListsService _wishListsService;
        private readonly LocalizationService _localization;
        private readonly UserStateManager _userStateManager;

        public string Command => BotCallbacks.AddItem;

        public AddItemCallbackAction(ITelegramBotClient bot, WishListsService wishListsService, UserStateManager userStateManager, LocalizationService localizationService)
        {
            _bot = bot;
            _wishListsService = wishListsService;
            _localization = localizationService;
            _userStateManager = userStateManager;
        }

        public Task ExecuteMessage(Message message, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            var chatId = callback.Message!.Chat.Id;
            try
            {
                if (callback == null)
                {
                    return;
                }
                var user = callback.From ?? throw new ArgumentNullException(nameof(callback.From));

                _userStateManager.SetState(user.Id, new AddingNewItemState(await _wishListsService.GetByName(user.Id, callback.Message.Text!)));
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.Cancel, user.Id), BotCallbacks.Cancel)
                    }
                });
                var mes = await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.SetItemName, user.Id), replyMarkup: keyboard, cancellationToken: token);
                UserContextManager.SetContext(user.Id, new UserContext(mes));
            }
            catch (Exception ex)
            {
                Log.Error($"Error AddItemCallbackAction: {ex.Message}");
            }
        }
    }
}
