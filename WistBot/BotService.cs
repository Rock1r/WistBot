using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WistBot.Res;
using WistBot.Services;

namespace WistBot
{
    public class BotService
    {
        private readonly ITelegramBotClient _client;
        private readonly Dictionary<string, Func<object, CancellationToken, Task>> _actions;
        private readonly UsersService _usersService;
        private readonly WishListsService _wishListsService;
        private readonly WishListItemsService _wishListItemsService;
        private readonly UserStateManager _userStateManager;
        private readonly LocalizationService _localization;
        private readonly ActionService _actionService;

        public BotService(ITelegramBotClient bot, 
            UsersService usersService, 
            WishListsService wishListService, 
            WishListItemsService wishListItemsService, 
            UserStateManager userStateManager, 
            LocalizationService localization,
            ActionService actionService
            )
        {
            _client = bot;
            _usersService = usersService;
            _wishListsService = wishListService;
            _wishListItemsService = wishListItemsService;
            _userStateManager = userStateManager;
            _localization = localization;
            _actionService = actionService;

            BotActions.Initialize(_usersService, _wishListsService, _wishListItemsService, _client, _userStateManager, _localization);

            _actions = InitializeActions();

        }

        public void StartReceiving()
        {
            _client.StartReceiving(HandleUpdate, HandleError);
        }

        private async Task HandleUpdate(ITelegramBotClient _client, Update update, CancellationToken token)
        {
            if (update.Message is { } message)
            {
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
                    await _client.SendMessage(message.Chat.Id, "Unknown command", cancellationToken: token);
                }
            }
            else if (update.CallbackQuery is { } callback)
            {
                try
                {
                    await _actionService.ExecuteCallback(callback.Data ?? string.Empty, callback, token);
                }
                catch (KeyNotFoundException)
                {
                    await _client.AnswerCallbackQuery(callback.Id, "Unknown action", cancellationToken: token);
                }
            }
        }

        private Dictionary<string, Func<object, CancellationToken, Task>> InitializeActions()
        {
            var actions = new Dictionary<string, Func<object, CancellationToken, Task>>
            {
                [BotCommands.Start] = async (obj, token) =>
                {
                    var msg = obj as Message ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.StartAction(msg, token);
                },
                [BotCommands.Language] = async (obj, token) =>
                {
                    var msg = obj as Message ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.LanguageAction(msg, token);
                },
                [Button.Ukrainian] = async (obj, token) =>
                {
                    var msg = obj as Message ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.ChangeLanguageButton(msg, token, LanguageCodes.Ukrainian);
                },
                [Button.English] = async (obj, token) =>
                {
                    var msg = obj as Message ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.ChangeLanguageButton(msg, token, LanguageCodes.English);
                },
                [BotCommands.List] = async (obj, token) =>
                {
                    var msg = obj as Message ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.ListsAction(msg, token, _localization);
                },
                [BotCallbacks.List] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.ListCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.DeleteList] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.DeleteListCallbackAction(callback, token, _localization);
                },
                [_localization.Get(Button.AddList)] = async (obj, token) =>
                {
                    var msg = obj as Message ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.AddListAction(msg, token, _localization);
                },
                [BotCallbacks.ChangeVisіbility] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.ChangeListVisibilityCallbackAction(callback, token, _localization);
                },
                
                [BotCallbacks.SetName] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.SetItemNameCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.SetDescription] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.SetItemDescriptionCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.SetLink] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.SetItemLinkCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.SetMedia] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.SetItemMediaCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.UserList] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.UserListCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.DeleteItem] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.DeleteItemCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.ChangeListName] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery ?? throw new ArgumentNullException(nameof(obj));
                    await BotActions.ChangeListNameCallbackAction(callback, token, _localization);
                },
                [BotCommands.Test] = async (obj, token) =>
                    {
                        var msg = obj as Message ?? throw new ArgumentNullException(nameof(obj));

                        await BotActions.Test(msg, token, _localization);
                },
            };

            return actions;
        }

       /* private async Task HandleUpdate(ITelegramBotClient bot, Update upd, CancellationToken token)
        {
            try
            {
                switch (upd.Type)
                {
                    case UpdateType.Message:
                        await HandleMessage(upd.Message, bot, token);
                        break;

                    case UpdateType.CallbackQuery:
                        await HandleCallbackQuery(upd.CallbackQuery, bot, token);
                        break;

                    default:
                        Console.WriteLine($"Unsupported UpdateType: {upd.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing update: {ex}");
            }
        }
*/
        private async Task HandleMessage(Message message, ITelegramBotClient bot, CancellationToken token)
        {
            if (message == null)
                return;

            var chatId = message.Chat.Id;
            var userId = message.From.Id;
            if (_userStateManager.UserHasState(userId))
            {
                await _userStateManager.HandleStateAsync(userId, message, bot, token, _localization, _wishListsService, _wishListItemsService);
                return;
            }

            if (message.Text == null)
                return;
            if (message.Text.StartsWith("@"))
            {
                await BotActions.ShowUserLists(message, token, _localization);
                return;
            }

            if (_actions.TryGetValue(message.Text.Trim(), out var action))
            {
                await action(message, token);
            }
            else
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.NotACommand), cancellationToken: token);
            }
        }

        private async Task HandleCallbackQuery(CallbackQuery callback, ITelegramBotClient bot, CancellationToken token)
        {
            if (callback == null || callback.Data == null)
                return;

            var dataParts = callback.Data.Split(':');
            var key = dataParts[0];

            if (_actions.TryGetValue(key, out var action))
            {
                await action(callback, token);
            }
            else
            {
                var chatId = callback.Message?.Chat.Id;
                if (chatId != null)
                {
                    await bot.SendMessage(chatId.Value, _localization.Get(LocalizationKeys.NotACommand), cancellationToken: token);
                }
            }
        }


        private Task HandleError(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}