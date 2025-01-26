using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Res;
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
                var keyboard = new InlineKeyboardMarkup(new InlineKeyboardButton
                {
                    Text = await _localization.Get(InlineButton.AddList, user.Id),
                    CallbackData = BotCallbacks.AddList
                });
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
                    var inlineReply = await WishListsService.GetListMarkup(list, _localization);
                    await _bot.SendMessage(message.Chat.Id, list.Name, replyMarkup: inlineReply, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
                Log.Information("ListsAction executed for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error ListsAction");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
