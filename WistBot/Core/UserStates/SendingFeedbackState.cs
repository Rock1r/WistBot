using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.UserStates
{
    public class SendingFeedbackState : IUserStateHandler
    {
        private readonly UsersService _usersService;
        public SendingFeedbackState(UsersService usersService)
        {
            _usersService = usersService;
        }

        public async Task<bool> HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, ItemsService wishListItemsService)
        {
            try
            {
                var chatId = message.Chat.Id;
                var user = message.From ?? throw new ArgumentNullException(nameof(message.From));
                await bot.SendMessage(chatId, await localization.Get(LocalizationKeys.FeedbackSent, userId), cancellationToken: token);
                var admin = await _usersService.GetByUsername("rockir");
                await bot.ForwardMessage(admin.ChatId, chatId, message.MessageId, cancellationToken: token);
                Log.Information("Feedback from user {UserId} sent to admin", user.Id);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error SendingFeedbackState");
                return false; // Ensure a return value in case of exception
            }
        }
    }
}
