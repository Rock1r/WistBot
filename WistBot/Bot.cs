using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WistBot.src;

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
                [Button.AddList] = async (obj, bot, token) =>
                {
                    var msg = obj as Message;
                    await BotActions.AddListAction(msg, bot, token, _localization, _database);
                },
                [BotCallbacks.AddItem] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.AddListItemCallbackAction(callback, bot, token, _localization, _database);
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
                /*[BotCallbacks.SetLink] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.SetItemLinkCallbackAction(callback, bot, token, _localization, _database);
                },*/
                [BotCallbacks.SetMedia] = async (obj, bot, token) =>
                {
                    var callback = obj as CallbackQuery;
                    await BotActions.SetItemMediaCallbackAction(callback, bot, token, _localization, _database);
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
                var type = upd.Type;
                switch (type)
                {
                    case UpdateType.Message:
                        var message = upd.Message;
                        var chatId = message.Chat.Id;
                        var userId = message.From.Id;

                        if (UserStateManager.UserHasState(userId))
                        {
                            await UserStateManager.HandleStates(userId, message, bot, token, _localization, _database);

                            return;
                        }
                        if (message.Text == null)
                        {
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
                        break;

                    case UpdateType.CallbackQuery:
                        var callback = upd.CallbackQuery;
                        if (_actions.TryGetValue(callback.Data, out var ction))
                        {
                            await ction(callback, bot, token);
                        }
                        else
                        {
                            await bot.SendMessage(callback.Message.Chat.Id, _localization.Get(LocalizationKeys.NotACommand), cancellationToken: token);
                        }
                        break;

                    default:
                        Console.WriteLine($"Unsupported UpdateType: {type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing update: {ex.Message}");
            }
        }
        
        private Task HandleError(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}