using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Core.UserStates;
using WistBot.Data.Models;
using WistBot.Managers;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class MyPresentsAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly UsersService _usersService;
        private readonly ItemsService _itemsService;

        public string Command => BotCommands.MyPresents;

        public MyPresentsAction(ITelegramBotClient bot, LocalizationService localizationService, UsersService usersService, ItemsService itemsService)
        {
            _bot = bot;
            _localization = localizationService;
            _usersService = usersService;
            _itemsService = itemsService;
        }

        public async Task ExecuteMessage(Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;
            try
            {
                var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                var username = user.Username ?? user.FirstName;
                var presents = await _itemsService.GetPresents(username);
                if (presents.Count == 0)
                {
                    await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.NoPresents, user.Id), replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
                    return;
                }
                foreach (var kvp in presents)
                {
                    long receiverId = kvp.Key;
                    List<ItemEntity> gifts = kvp.Value;

                        var receiver = await _usersService.GetById(receiverId);
                        var receiverUsername = receiver.Username;
                    await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.PresentsFor, user.Id, receiverUsername), cancellationToken: token);
                    foreach (var gift in gifts)
                    {
                        await ItemsService.BuildUserItemMarkup(user, _localization, receiverUsername, gift);
                        await ItemsService.ViewAnotherUserItem(_bot, chatId, user, gift, _localization, _usersService, token);
                    }
                }
                Log.Information("MyPresentsAction executed for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error MyPresentsAction");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
