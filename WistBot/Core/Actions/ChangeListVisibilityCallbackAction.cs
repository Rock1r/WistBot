using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Core.UserStates;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class ChangeListVisibilityCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly WishListsService _wishListsService;
        private readonly BotService _botService;

        public string Command => BotCallbacks.ChangeVisіbility;

        public ChangeListVisibilityCallbackAction(ITelegramBotClient bot, LocalizationService localizationService, WishListsService wishListsService, BotService botService)
        {
            _bot = bot;
            _localization = localizationService;
            _wishListsService = wishListsService;
            _botService = botService;
        }

        public Task ExecuteMessage(Message message, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            try
            {
                if (callback == null)
                {
                    return;
                }
                var userId = callback.From.Id;
                var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
                var listName = message.Text ?? throw new ArgumentNullException(nameof(message.Text));
                var list = await _wishListsService.GetByName(userId, listName);
                list.IsPublic = !list.IsPublic;

                await _wishListsService.Update(list.Id, list.Name, list.IsPublic);
                var visibilityButtonText = await _localization.Get(InlineButton.ChangeVisіbility, userId, list.IsPublic ?
                        await _localization.Get(LocalizationKeys.MakePrivate, userId) :
                        await _localization.Get(LocalizationKeys.MakePublic, userId));
                var inlineReply = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.WatchList, userId), BotCallbacks.List)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.DeleteList, userId), BotCallbacks.DeleteList),
                        //InlineKeyboardButton.WithCallbackData(await _localization.Get(Button.ShareList, user.Id), BotCallbacks.ShareList)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.ChangeListName, userId), BotCallbacks.ChangeListName),
                        InlineKeyboardButton.WithCallbackData(visibilityButtonText, BotCallbacks.ChangeVisіbility)
                    } });
                await _bot.EditMessageReplyMarkup(callback.Message.Chat.Id, callback.Message.Id, inlineReply);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ChangeListVisibilityCallbackAction: {ex.Message}");
            }
        }
    }
}
