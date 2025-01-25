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
    public class ChangeListNameCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly WishListsService _wishListsService;
        private readonly UserStateManager _userStateManager;
        private readonly UsersService _usersService;

        public string Command => BotCallbacks.ChangeListName;

        public ChangeListNameCallbackAction(ITelegramBotClient bot, LocalizationService localizationService, WishListsService wishListsService, UserStateManager userStateManager, UsersService usersService)
        {
            _bot = bot;
            _localization = localizationService;
            _wishListsService = wishListsService;
            _userStateManager = userStateManager;
            _usersService = usersService;
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
                var list = await _wishListsService.GetByName(userId, listName);
                _userStateManager.SetState(userId, new SettingListNameState(list, _usersService));
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.Cancel, userId), BotCallbacks.Cancel)
                    }
                });
                var mesToDel = await _bot.SendMessage(message.Chat.Id, await _localization.Get(LocalizationKeys.SetListName, userId), replyMarkup: keyboard, cancellationToken: token);
                UserContextManager.SetContext(userId, new UserContext(mesToDel) { MessageToEdit = message });
            }
            catch (ListNotFoundException ex)
            {
                await _bot.AnswerCallbackQuery(callback.Id, ex.Message, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ChangeListNameCallbackAction: {ex.Message}");
            }
        }
    }
}
