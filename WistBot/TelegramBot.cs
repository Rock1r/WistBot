using Telegram.Bot;
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

        private readonly Dictionary<long, (UserStates, ListObject)> _userStates = new();

        private enum UserStates
        {
            SettingName,
            SettingDescription,
            SettingLink,
            SettingPhoto,
        }

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
                    if(_database.GetUserData(message.From.Id) == null || _database.GetUserData(message.From.Id).Count == 0)
                    {
                        await bot.SendMessage(message.Chat.Id, message.From.Username.ToString() + ", " + _localization.Get("list_empty"), cancellationToken: token);
                        return;
                    }
                    await bot.SendMessage(message.Chat.Id, message.From.Username.ToString() + ", " + _localization.Get("list_message"), cancellationToken: token);
                    foreach (var item in _database.GetUserData(message.From.Id))
                    {
                        if (item.Photo != null || item.Document != null)
                        {
                            if (item.Photo != null)
                            {
                                await bot.SendPhoto(message.Chat.Id, item.Photo, item.Name + "\n" + item.Description + "\n" + item.Link, cancellationToken: token);
                            }
                            else
                            {
                                await bot.SendDocument(message.Chat.Id, item.Document, item.Name + "\n" + item.Description + "\n" + item.Link, cancellationToken: token);
                            }
                        }
                        else
                        {
                            await bot.SendMessage(message.Chat.Id, item.Name + "\n" + item.Description + "\n" + item.Link, cancellationToken: token);
                        }
                    }
                },
                ["/add"] = async (message, bot, token) =>
                {
                    var chatId = message.Chat.Id;
                    var userId = message.From.Id;

                    if (!_database.UserExists(userId))
                    {
                        _database.AddUser(userId);
                    }

                    await bot.SendMessage(chatId, _localization.Get("add_name"), cancellationToken: token);

                    _userStates[userId] = (UserStates.SettingName, new ListObject());
                },
                ["/clear"] = async (message, bot, token) =>
                {
                    _database.ClearUserData(message.From.Id);
                    await bot.SendMessage(message.Chat.Id, _localization.Get("list_cleared"), cancellationToken: token);
                },
                ["/remove"] = async (message, bot, token) =>
                {
                    await bot.SendMessage(message.Chat.Id, _localization.Get("remove_name"), cancellationToken: token);
                },
                ["/edit"] = async (message, bot, token) =>
                {
                    await bot.SendMessage(message.Chat.Id, _localization.Get("edit_name"), cancellationToken: token);
                },
                ["/help"] = async (message, bot, token) =>
                {
                    await bot.SendMessage(message.Chat.Id, _localization.Get("help_message"), cancellationToken: token);
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
                },
                ["set_description"] = async (callback, bot, token) =>
                {
                   
                    await bot.SendMessage(callback.Message.Chat.Id, _localization.Get("set_description"), cancellationToken: token);
                    _userStates[callback.From.Id] = (UserStates.SettingDescription, _userStates[callback.From.Id].Item2);
                },
                ["set_link"] = async (callback, bot, token) =>
                {
                   
                    await bot.SendMessage(callback.Message.Chat.Id, _localization.Get("set_link"), cancellationToken: token);
                    _userStates[callback.From.Id] = (UserStates.SettingLink, _userStates[callback.From.Id].Item2);
                },
                ["set_media"] = async (callback, bot, token) =>
                {
                    await bot.SendMessage(callback.Message.Chat.Id, _localization.Get("set_media"), cancellationToken: token);
                    _userStates[callback.From.Id] = (UserStates.SettingPhoto, _userStates[callback.From.Id].Item2);
                },
                ["finish"] = async (callback, bot, token) =>
                {
                    FinishSetting(callback.From.Id);
                    await bot.SendMessage(callback.Message.Chat.Id, _localization.Get("set_success"), cancellationToken: token);
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
            bool messageIsCommand = message.Text != null && message.Text.Trim().StartsWith("/");

            if (message.From != null)
            {
                if (_userStates.ContainsKey(message.From.Id))
                {
                    if (!messageIsCommand)
                    {
                        await HandleStates(bot, message, token);
                        return;
                    }
                    FinishSetting(message.From.Id);
                }
            }

            if (messageIsCommand)
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

        private async Task HandleStates(ITelegramBotClient bot, Message message, CancellationToken token)
        {
            var userId = message.From.Id;
            var chatId = message.Chat.Id;

            if (!_userStates.TryGetValue(userId, out var state) || state == (null, null))
            {
                await bot.SendMessage(chatId, _localization.Get("state_free"), cancellationToken: token);
                return;
            }

            var obj = _userStates[userId].Item2;

            switch (Enum.Parse<UserStates>(state.Item1.ToString()))
            {
                case UserStates.SettingName:
                    var name = message.Text;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        await bot.SendMessage(chatId, _localization.Get("invalid_name"), cancellationToken: token);
                        return;
                    }
                    obj.Name = name;
                    await bot.SendMessage(chatId, _localization.Get("name_saved"), cancellationToken: token);
                    break;

                case UserStates.SettingDescription:
                    var description = message.Text;
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        await bot.SendMessage(chatId, _localization.Get("invalid_description"), cancellationToken: token);
                        return;
                    }
                    obj.Description = description;

                    await bot.SendMessage(chatId, _localization.Get("description_saved"), cancellationToken: token);
                    break;

                case UserStates.SettingLink:
                    var link = message.Text;
                    if (!Uri.IsWellFormedUriString(link, UriKind.Absolute))
                    {
                        await bot.SendMessage(chatId, _localization.Get("invalid_link"), cancellationToken: token);
                        return;
                    }
                    obj.Link = link;


                    await bot.SendMessage(chatId, _localization.Get("link_saved"), cancellationToken: token);
                    break;

                case UserStates.SettingPhoto:
                    if (message.Photo != null)
                    {
                        if (message.Photo.Length == 0)
                        {
                            await bot.SendMessage(chatId, _localization.Get("photo_not_received"), cancellationToken: token);
                            return;
                        }
                        var photo = message.Photo.Last();
                        obj.Photo = photo;
                    }
                    else if (message.Document != null)
                    {
                        var document = message.Document;
                        obj.Document = document;
                    }
                    else
                    {
                        await bot.SendMessage(chatId, _localization.Get("photo_not_received"), cancellationToken: token);
                        return;
                    }
                    await bot.SendMessage(chatId, _localization.Get("photo_saved"), cancellationToken: token);
                    break;
                default:
                    await bot.SendMessage(chatId, _localization.Get("unknown_state"), cancellationToken: token);
                    break;
            }
            _database.UpdateItem(userId, obj);
            await ShowSettingItemMenu(chatId, bot, token);
        }

        private async Task ShowSettingItemMenu(long chatId, ITelegramBotClient bot, CancellationToken token)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localization.Get("set_name"), "set_name"),
                    InlineKeyboardButton.WithCallbackData(_localization.Get("set_description"), "set_description"),
                },
                new[]{
                    InlineKeyboardButton.WithCallbackData(_localization.Get("set_link"), "set_link"),
                    InlineKeyboardButton.WithCallbackData(_localization.Get("set_media"), "set_media"),
                },
                new[]{
                    InlineKeyboardButton.WithCallbackData(_localization.Get("finish"), "finish") }
            });
            await bot.SendMessage(chatId, _localization.Get("seting_menu"), replyMarkup: keyboard, cancellationToken: token);
        }

        private void FinishSetting(long userId)
        {
            if (_userStates.ContainsKey(userId))
                _userStates.Remove(userId);
        }

    }
}