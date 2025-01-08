using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace WistBot
{
    internal class Bot
    {
        private readonly TelegramBotClient _client;
        private readonly Localization _localization;
        private readonly Dictionary<string, Func<object, ITelegramBotClient, CancellationToken, Task>> _actions;
        private readonly Database _database;
        private string _databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db", "WistDb.db");

        public Bot(string token)
        {
            _client = new TelegramBotClient(token);
            _localization = new Localization(LanguageCodes.English);

            _actions = InitializeActions();

            _database = new Database(_databasePath);
        }

        public void StartReceiving()
        {
            _client.StartReceiving(HandleUpdate, HandleError);
        }

        private Dictionary<string, Func<object, ITelegramBotClient, CancellationToken, Task>> InitializeActions()
        {
            var actions = new Dictionary<string, Func<object, ITelegramBotClient, CancellationToken, Task>>
            {
                [BotCommands.Start] = async (obj, bot, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.StartAction(msg, bot, token, _localization, _database);
                },
                [BotCommands.Language] = async (obj, bot, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.LanguageAction(msg, bot, token, _localization);
                },
                [Button.Ukrainian] = async (obj, bot, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.ChangeLanguageButton(msg, bot, token, _localization, LanguageCodes.Ukrainian);
                },
                [Button.English] = async (obj, bot, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.ChangeLanguageButton(msg, bot, token, _localization, LanguageCodes.English);
                },
                [BotCommands.List] = async (obj, bot, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.ListsAction(msg, bot, token, _localization, _database);
                },
                [BotCallbacks.List] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.ListCallbackAction(callback, bot, token, _localization, _database);
                },
                [BotCallbacks.DeleteList] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.DeleteListCallbackAction(callback, bot, token, _localization, _database);
                },
                [_localization.Get(Button.AddList)] = async (obj, bot, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.AddListAction(msg, bot, token, _localization, _database);
                },
                [BotCallbacks.ChangeVisability] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.ChangeListVisibilityCallbackAction(callback, bot, token, _localization, _database);
                },
                [_localization.Get(Button.AddItem)] = async (obj, bot, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.AddListItemAction(msg, bot, token, _localization, _database);
                },
                [BotCallbacks.SetName] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.SetItemNameCallbackAction(callback, bot, token, _localization, _database);
                },
                [BotCallbacks.SetDescription] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.SetItemDescriptionCallbackAction(callback, bot, token, _localization, _database);
                },
                [BotCallbacks.SetLink] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.SetItemLinkCallbackAction(callback, bot, token, _localization, _database);
                },
                [BotCallbacks.SetMedia] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.SetItemMediaCallbackAction(callback, bot, token, _localization, _database);
                },
                [BotCallbacks.UserList] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.UserListCallbackAction(callback, bot, token, _localization, _database);
                },
                [BotCallbacks.DeleteItem] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.DeleteItemCallbackAction(callback, bot, token, _localization, _database);
                },
                [BotCallbacks.ChangeListName] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.ChangeListNameCallbackAction(callback, bot, token, _localization, _database);
                },
                [BotCommands.Test] = async (obj, bot, token) =>
                    {
                        var msg = obj as Message;

                        await BotActions.Test(msg, bot, token, _localization, _database);
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

            if (UserStateManager.UserHasState(userId) && UserStateManager.GetState(userId).Item1 != UserStateManager.UserState.Free)
            {
                await UserStateManager.HandleStates(userId, message, bot, token, _localization, _database);
                return;
            }

            if (message.Text == null)
                return;
            if (message.Text.StartsWith("@"))
            {
                await BotActions.ShowUserLists(message, bot, token, _localization, _database);
                return;
            }

            if (_actions.TryGetValue(message.Text.Trim(), out var action))
            {
                await action(message, bot, token);
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
                await action(callback, bot, token);
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