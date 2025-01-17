using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class ListsAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly WishListsService _wishListsService;
        private readonly LocalizationService _localization;

        public string Command => BotCommands.Lists;

        public ListsAction(ITelegramBotClient bot, WishListsService wishListsService, LocalizationService localizationService)
        {
            _bot = bot;
            _wishListsService = wishListsService;
            _localization = localizationService;
        }

        public async Task ExecuteMessage(Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;
            try
            {
                var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                var keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(await _localization.Get(KButton.AddList, user.Id)));
                var wishLists = await _wishListsService.GetByOwnerId(user.Id);
                var username = user.Username ?? user.FirstName;
                var messageToSend = string.Empty;
                if (wishLists.Count != 0)
                {
                    messageToSend = await _localization.Get(LocalizationKeys.ListsMessage, user.Id, username);
                }
                else
                {
                    messageToSend = await _localization.Get(LocalizationKeys.NoLists, user.Id, username);  
                }
                await _bot.SendMessage(message.Chat.Id, messageToSend, replyMarkup: keyboard, cancellationToken: token);
                foreach (var list in wishLists)
                {
                    var visibilityButtonText = await _localization.Get(InlineButton.ChangeVisіbility, user.Id, list.IsPublic ? 
                        await _localization.Get(LocalizationKeys.MakePrivate, user.Id)  :
                        await _localization.Get(LocalizationKeys.MakePublic, user.Id));
                    var inlineReply = new InlineKeyboardMarkup(new[]
                    {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.WatchList, user.Id), BotCallbacks.List)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.DeleteList, user.Id), BotCallbacks.DeleteList),
                        //InlineKeyboardButton.WithCallbackData(await _localization.Get(Button.ShareList, user.Id), BotCallbacks.ShareList)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.ChangeListName, user.Id), BotCallbacks.ChangeListName),
                        InlineKeyboardButton.WithCallbackData(visibilityButtonText, BotCallbacks.ChangeVisіbility)
                    }
                });
                    await _bot.SendMessage(message.Chat.Id, list.Name, replyMarkup: inlineReply);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ListsAction: {ex.Message}");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
