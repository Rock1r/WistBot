using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class PresentCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly UsersService _usersService;
        private readonly ItemsService _itemsService;

        public string Command => BotCallbacks.WantToPresent;

        public PresentCallbackAction(ITelegramBotClient bot, ItemsService itemsService, LocalizationService localizationService, UsersService usersService)
        {
            _bot = bot;
            _localization = localizationService;
            _usersService = usersService;
            _itemsService = itemsService;
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
                var text = message.Text ?? message.Caption ?? throw new ArgumentNullException(nameof(message.Text));
                var itemName = text.Split('\n')[0];
                var item = await _itemsService.GetByName(user.TelegramId, itemName);
                item.PerformerName = sender.Username ?? sender.FirstName;
                item.CurrentState = Enums.State.InProcess;
                await _itemsService.Update(item);
                var messageToSend = await _localization.Get(LocalizationKeys.PromiseToPresent, sender.Id, sender.Username ?? sender.FirstName, user.Username, item.Name);
                await _bot.AnswerCallbackQuery(callback.Id, messageToSend, cancellationToken: token);
                await _bot.DeleteMessage(chatId, message.MessageId, cancellationToken: token);
                await ItemsService.ViewAnotherUserItem(_bot, chatId, sender, item, _localization, _usersService, token);
                Log.Information("PresentCallbackAction executed for user {UserId}", user.TelegramId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error PresentCallbackAction");
            }
        }
    }
}
