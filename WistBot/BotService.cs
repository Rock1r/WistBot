using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WistBot.Data.Repos;
using WistBot.Enums;
using WistBot.Services;

namespace WistBot
{
    internal class BotService
    {
        private readonly ITelegramBotClient _client;
        private readonly LocalizationService _localization;
        private readonly Dictionary<string, Func<object, CancellationToken, Task>> _actions;
        private readonly UsersService _usersService;
        private readonly WishListsService _wishListsService;
        private readonly WishListItemsService _wishListItemsService;
        private readonly UserStateManager _userStateManager;

        public BotService(ITelegramBotClient bot, UsersService usersService, WishListsService wishListService, WishListItemsService wishListItemsService, UserStateManager userStateManager)
        {
            _client = bot;
            _usersService = usersService;
            _wishListsService = wishListService;
            _wishListItemsService = wishListItemsService;
            _userStateManager = userStateManager;

            _localization = new LocalizationService(LanguageCodes.English);

            BotActions.Initialize(_usersService, _wishListsService, _wishListItemsService, _client, _userStateManager);

            _actions = InitializeActions();

        }

        public void StartReceiving()
        {
            _client.StartReceiving(HandleUpdate, HandleError);
        }

        private Dictionary<string, Func<object, CancellationToken, Task>> InitializeActions()
        {
            var actions = new Dictionary<string, Func<object, CancellationToken, Task>>
            {
                [BotCommands.Start] = async (obj, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.StartAction(msg, token, _localization);
                },
                [BotCommands.Language] = async (obj, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.LanguageAction(msg, token, _localization);
                },
                [Button.Ukrainian] = async (obj, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.ChangeLanguageButton(msg, token, _localization, LanguageCodes.Ukrainian);
                },
                [Button.English] = async (obj, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.ChangeLanguageButton(msg, token, _localization, LanguageCodes.English);
                },
                [BotCommands.List] = async (obj, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.ListsAction(msg, token, _localization);
                },
                [BotCallbacks.List] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.ListCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.DeleteList] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.DeleteListCallbackAction(callback, token, _localization);
                },
                [_localization.Get(Button.AddList)] = async (obj, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.AddListAction(msg, token, _localization);
                },
                [BotCallbacks.ChangeVisability] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.ChangeListVisibilityCallbackAction(callback, token, _localization);
                },
                
                [BotCallbacks.SetName] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.SetItemNameCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.SetDescription] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.SetItemDescriptionCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.SetLink] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.SetItemLinkCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.SetMedia] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.SetItemMediaCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.UserList] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.UserListCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.DeleteItem] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.DeleteItemCallbackAction(callback, token, _localization);
                },
                [BotCallbacks.ChangeListName] = async (obj, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.ChangeListNameCallbackAction(callback, token, _localization);
                },
                [BotCommands.Test] = async (obj, token) =>
                    {
                        var msg = obj as Message;

                        await BotActions.Test(msg, token, _localization);
                },
            };

            return actions;
        }

        private async Task HandleUpdate(ITelegramBotClient bot, Update upd, CancellationToken token)
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