using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Exceptions;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class ViewListCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly WishListsService _wishListsService;
        private readonly LocalizationService _localization;
        private readonly UsersService _usersService;

        public string Command => BotCallbacks.List;

        public ViewListCallbackAction(ITelegramBotClient bot, WishListsService wishListsService, UsersService usersService, LocalizationService localizationService)
        {
            _bot = bot;
            _wishListsService = wishListsService;
            _localization = localizationService;
            _usersService = usersService;
        }

        public Task ExecuteMessage(Message message, CancellationToken token)
        {
           return Task.CompletedTask;
        }
        
        public async Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            try
            {
                var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
                var chatId = message.Chat.Id;
                var user = callback.From ?? throw new ArgumentNullException(nameof(callback.From));
                var culture = await _localization.GetLanguage(callback.From.Id);
                var list = await _wishListsService.GetByName(user.Id, message.Text ?? throw new ArgumentNullException(nameof(message.Text)));
                var messageToSend = await _localization.Get(LocalizationKeys.ViewListMessage, user.Id, list.Name);
                if (!list.Items.Any())
                {
                    messageToSend = await _localization.Get(LocalizationKeys.ListIsEmpty, culture, user.Username ?? user.FirstName, list.Name);
                }
                _usersService.SetListContext(user.Id, list.Id);
                await _bot.AnswerCallbackQuery(callback.Id, messageToSend, cancellationToken: token);

                await _wishListsService.ViewList(_bot, chatId, user.Id, list, _localization, token);
            }
            catch(ListNotFoundException)
            {
                await _bot.AnswerCallbackQuery(callback.Id, await _localization.Get(LocalizationKeys.ListNotFound, callback.From.Id), cancellationToken: token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ViewListCallbackAction: {ex.Message}");
            }
        }
    }
}
