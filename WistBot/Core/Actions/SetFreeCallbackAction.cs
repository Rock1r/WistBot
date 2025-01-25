using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class SetFreeCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly UsersService _usersService;
        private readonly ItemsService _itemsService;

        public string Command => BotCallbacks.SetFree;

        public SetFreeCallbackAction(ITelegramBotClient bot, ItemsService itemsService, LocalizationService localizationService, UsersService usersService)
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
                var itemId = callback.Data?.Split(":")[1] ?? throw new ArgumentNullException(nameof(callback.Data));
                var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
                var chatId = message.Chat.Id;
                var sender = callback.From ?? throw new ArgumentNullException(nameof(callback.From));
                var culture = await _localization.GetLanguage(callback.From.Id);
                var item = await _itemsService.GetById(Guid.Parse(itemId));
                item.CurrentState = Enums.State.Free;
                item.PerformerName = string.Empty;
                await _itemsService.Update(item);

                var messageToSend = await _localization.Get(LocalizationKeys.MadeFree, sender.Id);
                await _bot.AnswerCallbackQuery(callback.Id, messageToSend, cancellationToken: token);
                await _bot.DeleteMessage(chatId, message.MessageId, cancellationToken: token);
                await ItemsService.ViewAnotherUserItem(_bot, chatId, sender, item, _localization, _usersService, token);
                Log.Information("SetFreeCallbackAction: User {UserId} set item {ItemId} free", sender.Id, item.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error SetFreeCallbackAction");
            }
        }
    }
}
