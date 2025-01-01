using System.Data.Entity;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.src;

namespace WistBot
{
    static class BotActions
    {
        private static IReplyMarkup keyboard = new ReplyKeyboardMarkup();
        public static async Task StartAction(Message message, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database db)
        {
            var chatId = message.Chat.Id;
            try
            {
                var userId = message.From.Id;
                if (!db.UserExists(userId))
                {
                    db.AddUser(userId, message.From.Username);
                }
                keyboard = new ReplyKeyboardRemove();
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.StartMessage), replyMarkup: keyboard, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error StartAction: {ex.Message}");
            }
        }

        public static async Task LanguageAction(Message message, ITelegramBotClient bot, CancellationToken token, Localization _localization)
        {
            var chatId = message.Chat.Id;
            keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(Button.Ukrainian), new KeyboardButton(Button.English));

            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.ChangeLanguage), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task ChangeLanguageButton(Message message, ITelegramBotClient bot, CancellationToken token, Localization _localization, string result)
        {
            _localization.SetLanguage(result);
            keyboard = new ReplyKeyboardRemove();
            await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.LanguageChanged), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task ShowList(Message message, ITelegramBotClient bot, CancellationToken token, Database _database, Localization _localization, string listName)
        {
            var userId = message.Chat.Id;
            var list = _database.GetWishList(userId, listName);
            keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(_localization.Get(Button.AddItem), BotCallbacks.AddItem),
                }
            });
            await bot.SendMessage(message.Chat.Id, listName, replyMarkup: keyboard, cancellationToken: token);
            if (list.Items.Count > 0)
            {
                foreach (var item in list.Items)
                {
                    var inlineReply = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(LocalizationKeys.SetName, BotCallbacks.SetName),
                            InlineKeyboardButton.WithCallbackData(LocalizationKeys.SetDescription, BotCallbacks.SetDescription)
                        }, new[]
                        {
                            InlineKeyboardButton.WithCallbackData(LocalizationKeys.SetMedia, BotCallbacks.SetMedia),
                            InlineKeyboardButton.WithCallbackData(LocalizationKeys.SetLink, BotCallbacks.SetLink)
                        }, new[]
                        {
                            InlineKeyboardButton.WithCallbackData(LocalizationKeys.DeleteItem, BotCallbacks.DeleteItem),
                        }
                    });
                    if (item.Photo is not null)
                    {
                        await bot.SendPhoto(message.Chat.Id, item.Photo, $"<b>{item.Name}</b>" + "\n" + item.Description + "\n" + item.Link, replyMarkup: inlineReply, cancellationToken: token, parseMode: ParseMode.Html);
                    }
                    else
                    {
                        await bot.SendMessage(message.Chat.Id, item.Name + "\n" + item.Description + "\n" + item.Link, replyMarkup: inlineReply, cancellationToken: token);
                    }
                }
            }
        }

        public static async Task ShowUserLists(Message message, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database _database)
        {
            var chatId = message.Chat.Id;
            var username = message.Text.Trim();

            if (username.StartsWith("@"))
            {
                username = username.Substring(1);
            }

            if (!_database.UserExists(username))
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.UserNotFound), cancellationToken: token);
                return;
            }

            var userId = _database.GetUserId(username);
            var userData = _database.GetUserData(userId);

            if (userData != null && userData.WishLists.Any())
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.UserListsMessage), cancellationToken: token);
                int counter = 0;

                foreach (var list in userData.WishLists)
                {
                    if (list.IsPublic)
                    {
                        counter++;
                        var inlineReply = new InlineKeyboardMarkup(new[]
                        {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(list.Name, BotCallbacks.List)
                    }
                });
                        await bot.SendMessage(chatId, list.Name, replyMarkup: inlineReply);
                    }
                }

                if (counter == 0)
                {
                    await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.NoPublicLists), cancellationToken: token);
                }
            }
            else
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.NoLists), cancellationToken: token);
            }
        }


        public static async Task ListsAction(Message message, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database _database)
        {
            var chatId = message.Chat.Id;

            keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(_localization.Get(Button.AddList)));
            var currentData = _database.GetUserData(message.From.Id);
            var messageToSend = message.From.Username.ToString() + ", ";
            if (currentData.WishLists.Count != 0)
            {
                messageToSend += _localization.Get(LocalizationKeys.ListsMessage);
            }
            else
            {
                messageToSend += _localization.Get(LocalizationKeys.NoList);
            }
            await bot.SendMessage(message.Chat.Id, messageToSend, replyMarkup: keyboard, cancellationToken: token);
            foreach (var list in currentData.WishLists)
            {
                var inlineReply = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(list.Name, BotCallbacks.List)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(_localization.Get(Button.DeleteList), BotCallbacks.DeleteList),
                        InlineKeyboardButton.WithCallbackData(_localization.Get(Button.ShareList), BotCallbacks.ShareList)
                    }
                });
                await bot.SendMessage(message.Chat.Id, list.Name, replyMarkup: inlineReply);
            }
        }

        public static async Task AddListAction(Message message, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database _database)
        {
            var chatId = message.Chat.Id;
            UserStateManager.SetState(message.From.Id, UserStateManager.UserState.SettingListName, new WishList());
            keyboard = new ReplyKeyboardRemove();
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.AddList), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task ListCallbackAction(CallbackQuery callback, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database _database)
        {
            if (callback == null)
            {
                return;
            }
            keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(Button.ClearList), new KeyboardButton(Button.ShareList), new KeyboardButton(Button.ChangeListName));
            await bot.SendMessage(callback.Message.Chat.Id, _localization.Get(LocalizationKeys.ListMessage), replyMarkup: keyboard, cancellationToken: token);
            await ShowList(callback.Message, bot, token, _database, _localization, callback.Message.Text);
        }

        public static async Task DeleteListCallbackAction(CallbackQuery callback, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database _database)
        {
            if (callback == null)
            {
                return;
            }
            var userId = callback.From.Id;
            var listName = callback.Message.Text;
            _database.DeleteWishList(userId, listName);
            await bot.SendMessage(callback.Message.Chat.Id, _localization.Get(LocalizationKeys.ListDeleted), cancellationToken: token);
        }

        public static async Task AddListItemCallbackAction(CallbackQuery callback, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database _database)
        {
            if (callback == null)
            {
                return;
            }
            var chatId = callback.Message.Chat.Id;
            UserStateManager.SetState(callback.From.Id, UserStateManager.UserState.SettingItemName, _database.GetWishList(callback.From.Id, callback.Message.Text));
            keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(LocalizationKeys.DefaultItemNaming));
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.AddItem), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task SetItemNameCallbackAction(CallbackQuery callback, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database _database)
        {
            if (callback == null)
            {
                return;
            }
            var chatId = callback.Message.Chat.Id;
            var userId = callback.From.Id;
            var text = callback.Message.Text ?? callback.Message.Caption;
            text = text.Split("\n")[0];
            var item = _database.GetItem(userId, text);
            UserStateManager.SetState(userId, UserStateManager.UserState.SettingItemName, item);
            keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(LocalizationKeys.DefaultItemNaming));
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.SetName), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task SetItemDescriptionCallbackAction(CallbackQuery callback, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database _database)
        {
            if (callback == null)
            {
                return;
            }
            var chatId = callback.Message.Chat.Id;
            var userId = callback.From.Id;
            var text = callback.Message.Text ?? callback.Message.Caption;
            text = text.Split("\n")[0];
            var item = _database.GetItem(userId, text);
            UserStateManager.SetState(userId, UserStateManager.UserState.SettingDescription, item);
            keyboard = new ReplyKeyboardRemove();
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.SetDescription), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task SetItemMediaCallbackAction(CallbackQuery callback, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database _database)
        {
            if (callback == null)
            {
                return;
            }
            var chatId = callback.Message.Chat.Id;
            var userId = callback.From.Id;
            var text = callback.Message.Text ?? callback.Message.Caption;
            text = text.Split("\n")[0];

            var item = _database.GetItem(userId, text);
            UserStateManager.SetState(userId, UserStateManager.UserState.SettingMedia, item);
            keyboard = new ReplyKeyboardRemove();
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.SetMedia), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task Test(object msg, ITelegramBotClient bot, CancellationToken token, Localization _localization, Database database)
        {
            var message = msg as Message;
            var user = await bot.GetChatMember(message.Chat.Id, message.From.Id);
            Console.WriteLine("successfully.");

        }
    }
}
