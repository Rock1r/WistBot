using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class StartAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly UsersService _usersService;
        private readonly LocalizationService _localization;

        public string Command => BotCommands.Start;

        public StartAction(ITelegramBotClient bot, UsersService usersService, LocalizationService localizationService)
        {
            _bot = bot;
            _usersService = usersService;
            _localization = localizationService;
        }

        public async Task ExecuteMessage(Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;
            try
            {
                var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                if (!await _usersService.UserExists(user.Id))
                {
                    await _usersService.Add(user.Id, message.Chat.Id, message.From.Username ?? message.From.FirstName ?? message.From.LastName ?? "Unknown");
                    Log.Information("User {UserId} added", user.Id);
                }
                await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.StartMessage, user.Id), replyMarkup: new ReplyKeyboardRemove(), cancellationToken: token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error StartAction");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
