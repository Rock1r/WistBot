using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace WistBot
{
    internal class TelegramBot
    {
        private readonly TelegramBotClient _client;
        private readonly Localization _localization;
        private readonly Dictionary<string, Func<Message, ITelegramBotClient, CancellationToken, Task>> _commands;
        private readonly Dictionary<string, Func<CallbackQuery, ITelegramBotClient, CancellationToken, Task>> _callbacks;
        private readonly Database _database;
        private string _databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db", "WistDb.db");


        public TelegramBot(string token)
        {
            _client = new TelegramBotClient(token);
            _localization = new Localization("en");

            _commands = InitializeCommands();

            _callbacks = InitializeCallbacks();

            _database = new Database(_databasePath);
        }

        public void StartReceiving()
        {
            _client.StartReceiving(HandleUpdate, HandleError);
        }

        private Dictionary<string, Func<Message, ITelegramBotClient, CancellationToken, Task>> InitializeCommands()
        {
            var commands = new Dictionary<string, Func<Message, ITelegramBotClient, CancellationToken, Task>>
            {
                ["/start"] = async (message, bot, token) =>
                {
                    await bot.SendMessage(message.Chat.Id, _localization.Get("start_message"), cancellationToken: token);
                },
                ["/language"] = async (message, bot, token) =>
                {
                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("English", "en"),
                            InlineKeyboardButton.WithCallbackData("Ukrainian", "uk")
                        }
                    });
                    await bot.SendMessage(message.Chat.Id, _localization.Get("change_language"), replyMarkup: keyboard, cancellationToken: token);
                },
                ["/list"] = async (message, bot, token) =>
                {
                    if(_database.GetUserData(message.From.Id.ToString()) == null || _database.GetUserData(message.From.Id.ToString()).Count == 0)
                    {
                        await bot.SendMessage(message.Chat.Id, message.From.Username.ToString() + ", " + _localization.Get("list_empty"), cancellationToken: token);
                        return;
                    }
                    await bot.SendMessage(message.Chat.Id, message.From.Username.ToString() + ", " + _localization.Get("list_message"), cancellationToken: token);
                }
            };

            return commands;
        }

        private Dictionary<string, Func<CallbackQuery, ITelegramBotClient, CancellationToken, Task>> InitializeCallbacks()
        {
            var callbacks = new Dictionary<string, Func<CallbackQuery, ITelegramBotClient, CancellationToken, Task>>
            {
                ["en"] = async (callback, bot, token) =>
                {
                    _localization.SetLanguage("en");
                    await bot.SendMessage(callback.Message.Chat.Id, _localization.Get("language_changed"), cancellationToken: token);
                },
                ["uk"] = async (callback, bot, token) =>
                {
                    _localization.SetLanguage("uk");
                    await bot.SendMessage(callback.Message.Chat.Id, _localization.Get("language_changed"), cancellationToken: token);
                }
            };

            return callbacks;
        }

        private async Task HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            try
            {
                if (update.Message != null)
                {
                    await HandleMessage(bot, update.Message, token);
                }
                else if (update.CallbackQuery != null)
                {
                    await HandleCallback(bot, update.CallbackQuery, token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing update: {ex.Message}");
            }
        }

        private async Task HandleMessage(ITelegramBotClient bot, Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;

            if (message.Text is not null)
            {
                var command = message.Text.Trim().ToLower();

                if (_commands.TryGetValue(command, out var action))
                {
                    await action(message, bot, token);
                }
                else
                {
                    await bot.SendMessage(chatId, _localization.Get("not_a_command"), cancellationToken: token);
                }
            }
        }

        private async Task HandleCallback(ITelegramBotClient bot, CallbackQuery callback, CancellationToken token)
        {
            var chatId = callback.Message.Chat.Id;

            if (callback.Data is not null && _callbacks.TryGetValue(callback.Data, out var action))
            {
                await action(callback, bot, token);
            }
        }

        private Task HandleError(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}