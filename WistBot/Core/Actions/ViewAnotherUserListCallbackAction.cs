using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Exceptions;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class ViewAnotherUserListCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly WishListsService _wishListsService;
        private readonly LocalizationService _localization;
        private readonly UsersService _usersService;

        public string Command => BotCallbacks.ViewUserList;

        public ViewAnotherUserListCallbackAction(ITelegramBotClient bot, WishListsService wishListsService, LocalizationService localizationService, UsersService usersService)
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
                var username = callback.Data?.Split(":")[1] ?? throw new ArgumentNullException(nameof(callback.Data));
                var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
                var chatId = message.Chat.Id;
                var user = await _usersService.GetByUsername(username);
                var sender = callback.From ?? throw new ArgumentNullException(nameof(callback.From));
                var culture = await _localization.GetLanguage(callback.From.Id);
                var list = await _wishListsService.GetByName(user.TelegramId, message.Text ?? throw new ArgumentNullException(nameof(message.Text)));
                var messageToSend = await _localization.Get(LocalizationKeys.ViewListMessage, sender.Id, list.Name);
                if (!list.Items.Any())
                {
                    messageToSend = await _localization.Get(LocalizationKeys.UserListIsEmpty, culture, sender.Username ?? sender.FirstName, user.Username, list.Name);
                }
                await _bot.AnswerCallbackQuery(callback.Id, messageToSend, cancellationToken: token);

                await WishListsService.ViewUserList(_bot, chatId, sender, list, _localization, _usersService, token);
                Log.Information("User {UserId} viewed list {ListName} of user {Username}", sender.Id, list.Name, user.Username);
            }
            catch (ListNotFoundException)
            {
                await _bot.AnswerCallbackQuery(callback.Id, await _localization.Get(LocalizationKeys.ListNotFound, callback.From.Id), cancellationToken: token);
                Log.Information("List not found");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error ViewAnotherUserListCallbackAction");
            }
        }
    }
}
