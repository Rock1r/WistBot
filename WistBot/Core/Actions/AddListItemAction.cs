using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Core.UserStates;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class AddListItemAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly WishListsService _wishListsService;
        private readonly LocalizationService _localization;
        private readonly UserStateManager _userStateManager;
        private readonly UsersService _usersService;

        public string Command => KButton.AddItem;

        public AddListItemAction(ITelegramBotClient bot, WishListsService wishListsService, UserStateManager userStateManager, UsersService usersService, LocalizationService localizationService)
        {
            _bot = bot;
            _wishListsService = wishListsService;
            _localization = localizationService;
            _userStateManager = userStateManager;
            _usersService = usersService;
        }

        public async Task ExecuteMessage(Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;
            try
            {
                if (message == null)
                {
                    return;
                }
                var user = message.From ?? throw new ArgumentNullException(nameof(message.From));

                _userStateManager.SetState(user.Id, new AddingNewItemState(await _wishListsService.GetById(_usersService.GetListContext(user.Id))));

                var keyboard = new ReplyKeyboardMarkup(true).AddButtons(new KeyboardButton(LocalizationKeys.DefaultItemNaming));
                await _bot.SendMessage(chatId, _localization.Get(LocalizationKeys.AddItem), replyMarkup: keyboard, cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error AddListItemAction: {ex.Message}");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
