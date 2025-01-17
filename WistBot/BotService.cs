using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Res;
using WistBot.Services;

namespace WistBot
{
    public class BotService
    {
        private readonly ITelegramBotClient _client;
        private readonly UsersService _usersService;
        private readonly WishListsService _wishListsService;
        private readonly WishListItemsService _wishListItemsService;
        private readonly UserStateManager _userStateManager;
        private readonly LocalizationService _localization;
        private readonly ActionService _actionService;

        public BotService(ITelegramBotClient bot, 
            UsersService usersService, 
            WishListsService wishListService, 
            WishListItemsService wishListItemsService, 
            UserStateManager userStateManager, 
            LocalizationService localization,
            ActionService actionService
            )
        {
            _client = bot;
            _usersService = usersService;
            _wishListsService = wishListService;
            _wishListItemsService = wishListItemsService;
            _userStateManager = userStateManager;
            _localization = localization;
            _actionService = actionService;

            BotActions.Initialize(_usersService, _wishListsService, _wishListItemsService, _client, _userStateManager, _localization);


        }

        public void StartReceiving()
        {
            _client.StartReceiving(HandleUpdate, HandleError);
        }

        private async Task HandleUpdate(ITelegramBotClient _client, Update update, CancellationToken token)
        {
            if (update.Message is { } message)
            {
                try
                {
                    var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                    if (_userStateManager.UserHasState(user.Id))
                    {
                        await _userStateManager.HandleStateAsync(user.Id, message, _client, token, _localization, _wishListsService, _wishListItemsService);
                        return;
                    }
                    await _actionService.ExecuteMessage(message.Text ?? string.Empty, message, token);
                }
                catch (KeyNotFoundException)
                {
                    await _client.SendMessage(message.Chat.Id, "Unknown command", cancellationToken: token);
                }
            }
            else if (update.CallbackQuery is { } callback)
            {
                try
                {
                    await _actionService.ExecuteCallback(callback.Data ?? string.Empty, callback, token);
                }
                catch (KeyNotFoundException)
                {
                    await _client.AnswerCallbackQuery(callback.Id, "Unknown action", cancellationToken: token);
                }
            }
        }

        private Task HandleError(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }

        public async Task<InlineKeyboardMarkup> BuildInlineMarkupForList(long userId, string listName)//TODO: move to another class
        {
            var list = await _wishListsService.GetByName(userId, listName);
            var visibilityButtonText = await _localization.Get(InlineButton.ChangeVisіbility, userId, list.IsPublic ?
                    await _localization.Get(LocalizationKeys.MakePrivate, userId) :
                    await _localization.Get(LocalizationKeys.MakePublic, userId));
            return new InlineKeyboardMarkup(new[]
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
                }
            });
        }
    }
}