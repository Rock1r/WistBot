using Microsoft.AspNetCore.Cors.Infrastructure;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Exceptions;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class FindUserAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly WishListsService _wishListsService;
        private readonly LocalizationService _localization;
        private readonly UsersService _usersService;

        public FindUserAction(ITelegramBotClient botClient, WishListsService wishListsService, LocalizationService localization, UsersService usersService)
        {
            _bot = botClient;
            _wishListsService = wishListsService;
            _localization = localization;
            _usersService = usersService;
        }

        public string Command => "/finduser";

        public async Task ExecuteMessage(Message message, CancellationToken token)
        {
            var chatId = message.Chat.Id;
            var senderId = message.From.Id;
            try
            {
                string username = message.Text.Trim().Replace("@", ""); // Видаляємо @ якщо є
                if (!await _usersService.UserExists(username))
                {
                    await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.UserNotFound, senderId), cancellationToken: token);
                    return;
                }
                var userId = await _usersService.GetId(username);
                var lists = await _wishListsService.GetPublic(userId);
                if (lists != null && lists.Count > 0)
                {
                    await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.UsersListsMessage, senderId, username), cancellationToken: token);

                    foreach (var list in lists)
                    {

                        var inlineReply = new InlineKeyboardMarkup(new[]
                        {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.WatchList, senderId), $"{BotCallbacks.ViewUserList}:{username}")
                    }
                });
                        await _bot.SendMessage(chatId, list.Name, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: inlineReply, cancellationToken: token);
                    }
                }
                else
                {
                    await _bot.SendMessage(chatId, await _localization.Get(LocalizationKeys.NoPublicLists, senderId, username),  cancellationToken: token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to find user: {ex.Message}");
            }
        }

        public Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }

}
