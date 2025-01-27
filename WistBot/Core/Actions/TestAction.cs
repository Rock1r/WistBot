using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class TestAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly UsersService _usersService;
        private readonly WishListsService _wishListsService;
        private readonly ItemsService _itemsService;

        public string Command => BotCommands.test;

        public TestAction(ITelegramBotClient bot, LocalizationService localizationService, UsersService usersService, WishListsService wishListsService, ItemsService itemsService)
        {
            _bot = bot;
            _localization = localizationService;
            _usersService = usersService;
            _wishListsService = wishListsService;
            _itemsService = itemsService;
        }

        public async Task ExecuteMessage(Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;
            try
            {
                
                Log.Information("Test executed");

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error test");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
