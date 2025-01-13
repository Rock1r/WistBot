using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Data.Models;
using WistBot.Services;
using WistBot.States;
using WistBot.UserStates;

namespace WistBot
{
    static class BotActions
    {
        private static IReplyMarkup keyboard = new ReplyKeyboardMarkup();
        private static UsersService? _usersService;
        private static WishListsService? _wishListsService;
        private static WishListItemsService? _wishListItemsService;
        private static ITelegramBotClient? bot;
        private static UserStateManager? _userStateManager;

        public static void Initialize(UsersService usersService, WishListsService wishListsService, WishListItemsService wishListItemsService, ITelegramBotClient telegramBotClient, UserStateManager userStateManager)
        {
            _usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
            _wishListsService = wishListsService ?? throw new ArgumentNullException(nameof(wishListsService));
            _wishListItemsService = wishListItemsService ?? throw new ArgumentNullException(nameof(wishListItemsService));
            bot = telegramBotClient ?? throw new ArgumentNullException(nameof(telegramBotClient));
            _userStateManager = userStateManager ?? throw new ArgumentNullException(nameof(userStateManager));
        }

        public static async Task StartAction(Message message, CancellationToken token, LocalizationService _localization)
        {
            var chatId = message.Chat.Id;
            try
            {
                var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                if (!await _usersService.UserExists(user.Id))
                {
                    await _usersService.Add(user.Id, message.From.Username ?? throw new Exception("Username is null"));
                }
                keyboard = new ReplyKeyboardRemove();
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.StartMessage), replyMarkup: keyboard, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error StartAction: {ex.Message}");
            }
        }

        public static async Task LanguageAction(Message message, CancellationToken token, LocalizationService _localization)
        {
            var chatId = message.Chat.Id;
            keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(Button.Ukrainian), new KeyboardButton(Button.English));
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.ChangeLanguage), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task ChangeLanguageButton(Message message, CancellationToken token, LocalizationService _localization, string result)
        {
            await _usersService.SetLanguage(message.From.Id, result);
            keyboard = new ReplyKeyboardRemove();
            await bot.SendMessage(message.Chat.Id, _localization.Get(LocalizationKeys.LanguageChanged), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task ShowList(Message message, CancellationToken token, LocalizationService _localization, WishListEntity list)
        {
            var userId = message.Chat.Id;
            keyboard = new ReplyKeyboardMarkup(new KeyboardButton[][]
            {
                 new KeyboardButton[] { Button.AddItem  },
                 new KeyboardButton[] { Button.ClearList}
            })
            {
                ResizeKeyboard = true
            };
            await bot.SendMessage(message.Chat.Id, list.Name, replyMarkup: keyboard, cancellationToken: token);
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
                    if (!string.IsNullOrWhiteSpace(item.Media))
                    {
                        var file = await bot.GetFile(item.Media);
                        if (file != null)
                        {
                            if (file.FilePath.EndsWith(".jpg") || file.FilePath.EndsWith(".png"))
                            {
                                await bot.SendPhoto(message.Chat.Id, item.Media, $"<b>{item.Name}</b>" + "\n" + item.Description + "\n" + item.Link, replyMarkup: inlineReply, cancellationToken: token, parseMode: ParseMode.Html);
                            }
                            else
                            {
                                await bot.SendVideo(message.Chat.Id, item.Media, $"<b>{item.Name}</b>" + "\n" + item.Description + "\n" + item.Link, replyMarkup: inlineReply, cancellationToken: token, parseMode: ParseMode.Html);
                            }
                        }
                    }
                    else
                    {
                        await bot.SendMessage(message.Chat.Id, $"<b>{item.Name}</b>" + "\n" + item.Description + "\n" + item.Link, replyMarkup: inlineReply, cancellationToken: token, parseMode: ParseMode.Html);
                    }
                }
            }
        }

        public static async Task ShowUserLists(Message message, CancellationToken token, LocalizationService _localization)
        {
            var chatId = message.Chat.Id;
            var username = message.Text.Trim();

            if (username.StartsWith("@"))
            {
                username = username.Substring(1);
            }
            keyboard = new ReplyKeyboardRemove();
            if (!await _usersService.UserExists(await _usersService.GetId(username)))
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.UserNotFound), replyMarkup: keyboard, cancellationToken: token);
                return;
            }

            var userId = await _usersService.GetId(username);
            var lists = await _wishListsService.GetByOwnerId(userId);

            if (lists.Any())
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.UserListsMessage), replyMarkup: keyboard, cancellationToken: token);
                bool isPublic = false;
                if (lists.Any(list => list.IsPublic))
                {
                    isPublic = true;
                }
                foreach (var list in lists)
                {
                    if (list.IsPublic)
                    {
                        var inlineReply = new InlineKeyboardMarkup(new[]
                        {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(list.Name, BotCallbacks.UserList + $":{username}")
                    }
                });
                        await bot.SendMessage(chatId, list.Name, replyMarkup: inlineReply);
                    }
                }

                if (!isPublic)
                {
                    await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.NoPublicLists), replyMarkup: keyboard, cancellationToken: token);
                }
            }
            else
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.NoLists), replyMarkup: keyboard, cancellationToken: token);
            }
        }

        public static async Task ListsAction(Message message, CancellationToken token, LocalizationService _localization)
        {
            var chatId = message.Chat.Id;

            keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(_localization.Get(Button.AddList)));
            var wishLists = await _wishListsService.GetByOwnerId(message.From.Id);
            var messageToSend = message.From.Username.ToString() + ", ";
            if (wishLists.Count != 0)
            {
                messageToSend += _localization.Get(LocalizationKeys.ListsMessage);
            }
            else
            {
                messageToSend += _localization.Get(LocalizationKeys.NoList);
            }
            await bot.SendMessage(message.Chat.Id, messageToSend, replyMarkup: keyboard, cancellationToken: token);
            foreach (var list in wishLists)
            {
                var inlineReply = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(_localization.Get(LocalizationKeys.WatchList), BotCallbacks.List)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(_localization.Get(Button.DeleteList), BotCallbacks.DeleteList),
                        InlineKeyboardButton.WithCallbackData(_localization.Get(Button.ShareList), BotCallbacks.ShareList)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(_localization.Get(Button.ChangeListName), BotCallbacks.ChangeListName),
                        InlineKeyboardButton.WithCallbackData(_localization.Get(Button.ChangeVisability), BotCallbacks.ChangeVisability)
                    }
                });
                await bot.SendMessage(message.Chat.Id, list.Name, replyMarkup: inlineReply);
            }
        }

        public static async Task AddListAction(Message message, CancellationToken token, LocalizationService _localization)
        {
            var chatId = message.Chat.Id;
            _userStateManager.SetState(message.From.Id, new SettingListNameState(null));
            keyboard = new ReplyKeyboardRemove();
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.SetListName), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task ListCallbackAction(CallbackQuery callback, CancellationToken token, LocalizationService _localization)
        {
            if (callback == null)
            {
                return;
            }
            var user = callback.From ?? throw new ArgumentNullException(nameof(callback.From));
            var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
            var list = await _wishListsService.GetByName(user.Id, message.Text ?? throw new ArgumentNullException(nameof(message.Text)));
            if (list is null)
            {
                await bot.SendMessage(callback.Message.Chat.Id, _localization.Get(LocalizationKeys.ListNotFound), cancellationToken: token);
                return;
            }
            var messageToSend = _localization.Get(LocalizationKeys.ListMessage);
            if (!list.Items.Any())
            {
                messageToSend = _localization.Get(LocalizationKeys.EmptyList);
            }
            _userStateManager.SetState(user.Id, new ViewingListState(list, _userStateManager));
            await bot.SendMessage(callback.Message.Chat.Id, messageToSend, cancellationToken: token);
            await ShowList(callback.Message, token, _localization, await _wishListsService.GetByName(user.Id, callback.Message.Text));
        }

        public static async Task UserListCallbackAction(CallbackQuery callback, CancellationToken token, LocalizationService _localization)
        {
            if (callback == null)
            {
                return;
            }
            var username = callback.Data.Split(":")[1];
            var userId = callback.From.Id;
            var chatId = callback.Message.Chat.Id;
            var listName = callback.Message.Text;
            keyboard = new ReplyKeyboardRemove();
            await bot.SendMessage(callback.Message.Chat.Id, _localization.Get(LocalizationKeys.UserListMessage), cancellationToken: token);
            await ShowUserList(chatId, token, _localization, listName, username);
        }

        public static async Task ShowUserList(long chatId, CancellationToken token, LocalizationService _localization, string listName, string ownerName)
        {
            
            var list = await _wishListsService.GetByName(await _usersService.GetId(ownerName), listName);
            if (list is null)
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.ListNotFound), cancellationToken: token);
                return;
            }
            if (list.Items.Count > 0)
            {
                foreach (var item in list.Items)
                {
                    var name = item.Name;
                    if (item.Link is not null)
                    {
                        name = "<a href=\"" + item.Link + "\">" + item.Name + "</a>";
                    }
                    if (item.Media is not null)
                    {
                        await bot.SendPhoto(chatId, item.Media, $"<b>{name}</b>" + "\n" + item.Description, cancellationToken: token, parseMode: ParseMode.Html);
                    }
                    else
                    {
                        await bot.SendMessage(chatId, $"<b>{name}</b>" + "\n" + item.Description, cancellationToken: token, parseMode: ParseMode.Html);
                    }
                }
            }
        }

        public static async Task DeleteListCallbackAction(CallbackQuery callback, CancellationToken token, LocalizationService _localization)
        {
            if (callback == null)
            {
                return;
            }
            var userId = callback.From.Id;
            var listName = callback.Message.Text;
            await _wishListsService.Delete(listName);
            await bot.SendMessage(callback.Message.Chat.Id, _localization.Get(LocalizationKeys.ListDeleted) +"\n"+ _localization.Get(LocalizationKeys.Type_list), cancellationToken: token);
        }

        public static async Task ChangeListVisibilityCallbackAction(CallbackQuery callback, CancellationToken token, LocalizationService _localization)
        {
            if (callback == null)
            {
                return;
            }
            var userId = callback.From.Id;
            var listName = callback.Message.Text;
            var list = await _wishListsService.GetByName(userId, listName);
            if (list is null)
            {
                await bot.SendMessage(callback.Message.Chat.Id, _localization.Get(LocalizationKeys.ListNotFound), cancellationToken: token);
                return;
            }
            list.IsPublic = !list.IsPublic;
            var isPublic = list.IsPublic ? _localization.Get(LocalizationKeys.PublicVisibility) : _localization.Get(LocalizationKeys.PrivateVisibility);
            await _wishListsService.Update(list.Id, list.Name, list.IsPublic);
            await bot.SendMessage(callback.Message.Chat.Id, _localization.Get(LocalizationKeys.VisibilityChanged) + isPublic + "\n" + _localization.Get(LocalizationKeys.Type_list), cancellationToken: token);
        }

        public static async Task AddListItemAction(Message message, CancellationToken token, LocalizationService _localization, WishListEntity wishListEntity)
        {
            if (message == null)
            {
                return;
            }
            var chatId = message.Chat.Id;
            _userStateManager.SetState(message.From.Id, new AddingNewItemState(wishListEntity));
            keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(LocalizationKeys.DefaultItemNaming));
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.AddItem), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task SetItemNameCallbackAction(CallbackQuery callback, CancellationToken token, LocalizationService _localization)
        {
            if (callback == null)
            {
                return;
            }
            var chatId = callback.Message.Chat.Id;
            var userId = callback.From.Id;
            var text = callback.Message.Text ?? callback.Message.Caption;
            text = text.Split("\n")[0];
            var item = await _wishListItemsService.GetByName(text);
            if (item is null)
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.ItemNotFound), cancellationToken: token);
                return;
            }
            _userStateManager.SetState(userId, new SettingItemNameState(item));
            keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(LocalizationKeys.DefaultItemNaming));
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.SetName), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task SetItemDescriptionCallbackAction(CallbackQuery callback, CancellationToken token, LocalizationService _localization)
        {
            if (callback == null)
            {
                return;
            }
            var chatId = callback.Message.Chat.Id;
            var userId = callback.From.Id;
            var text = callback.Message.Text ?? callback.Message.Caption;
            text = text.Split("\n")[0];
            var item = await _wishListItemsService.GetByName(text);
            if (item is null)
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.ItemNotFound), cancellationToken: token);
                return;
            }
            _userStateManager.SetState(userId, new SettingDescriptionState(item));
            if (item.Description is not null)
            {
                keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(LocalizationKeys.RemoveDescription));
            }
            else
            {
                keyboard = new ReplyKeyboardRemove();
            }
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.SetDescription), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task SetItemMediaCallbackAction(CallbackQuery callback, CancellationToken token, LocalizationService _localization)
        {
            if (callback == null)
            {
                return;
            }
            var chatId = callback.Message.Chat.Id;
            var userId = callback.From.Id;
            var text = callback.Message.Text ?? callback.Message.Caption;
            text = text.Split("\n")[0];
            var item = await _wishListItemsService.GetByName(text);
            if (item is null)
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.ItemNotFound), cancellationToken: token);
                return;
            }
            _userStateManager.SetState(userId, new SettingMediaState(item));
            if (item.Media is not null)
            {
                keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(LocalizationKeys.RemoveMedia));
            }
            else
            {
                keyboard = new ReplyKeyboardRemove();
            }
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.SetMedia), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task SetItemLinkCallbackAction(CallbackQuery callback, CancellationToken token, LocalizationService _localization)
        {
            if (callback == null)
            {
                return;
            }
            var chatId = callback.Message.Chat.Id;
            var userId = callback.From.Id;
            var text = callback.Message.Text ?? callback.Message.Caption;
            text = text.Split("\n")[0];
            var item = await _wishListItemsService.GetByName(text);
            if (item is null)
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.ItemNotFound), cancellationToken: token);
                return;
            }
            _userStateManager.SetState(userId, new SettingLinkState(item));
            if (item.Link is not null)
            {
                keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(LocalizationKeys.RemoveLink));
            }
            else
            {
                keyboard = new ReplyKeyboardRemove();
            }
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.SetLink), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task DeleteItemCallbackAction(CallbackQuery callback, CancellationToken token, LocalizationService _localization)
        {
            if (callback == null)
            {
                return;
            }
            var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
            var chatId = message.Chat.Id;
            var userId = callback.From.Id;
            var text = callback.Message.Text ?? callback.Message.Caption ?? throw new ArgumentNullException(nameof(callback.Message.Text));
            text = text.Split("\n")[0];
            var item = await _wishListItemsService.GetByName(text);
            if (item is null)
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.ItemNotFound), cancellationToken: token);
                return;
            }
            await _wishListItemsService.Delete(item.Id);
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.ItemDeleted), cancellationToken: token);
            await ShowList(callback.Message, token, _localization, await _wishListsService.GetById(item.ListId));
        }

        public static async Task ChangeListNameCallbackAction(CallbackQuery callback, CancellationToken token, LocalizationService _localization)
        {
            var chatId = callback.Message.Chat.Id;
            var userId = callback.From.Id;
            var listName = callback.Message.Text;
            var list = await _wishListsService.GetByName(userId, listName);
            if (list is null)
            {
                await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.ListNotFound), cancellationToken: token);
                return;
            }
            _userStateManager.SetState(userId, new SettingListNameState(list));
            keyboard = new ReplyKeyboardMarkup(true).AddButton(new KeyboardButton(LocalizationKeys.DefaultListNaming));
            await bot.SendMessage(chatId, _localization.Get(LocalizationKeys.SetListName), replyMarkup: keyboard, cancellationToken: token);
        }

        public static async Task Test(object msg, CancellationToken token, LocalizationService _localization)
        {
            var message = msg as Message;
            var chatId = message.Chat.Id;
            var userId = message.From.Id;
            
            
            Console.WriteLine("successfully.");

        }
    }
}
