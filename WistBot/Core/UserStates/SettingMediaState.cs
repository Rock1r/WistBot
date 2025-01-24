using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;
using WistBot.Res;
using WistBot.Managers;
using WistBot.Enums;

namespace WistBot.Core.UserStates
{
    public class SettingMediaState : IUserStateHandler
    {
        private readonly ItemEntity _wishListItem;

        public SettingMediaState(ItemEntity wishListItem)
        {
            _wishListItem = wishListItem ?? throw new ArgumentNullException(nameof(wishListItem));
        }

        public async Task<bool> HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, ItemsService wishListItemsService)
        {
            try
            {
                bool itemHasMedia = !string.IsNullOrWhiteSpace(_wishListItem.Media);
                var context = UserContextManager.GetContext(userId) ?? throw new ArgumentNullException();
                if (message.Document != null)
                {
                    var warning = await bot.SendMessage(message.Chat.Id, await localization.Get(LocalizationKeys.DocumentNotSupported, userId), cancellationToken: token);
                    context.MessagesToDelete.Add(message);
                    return false;
                }

                if (message.Photo != null)
                {
                    _wishListItem.Media = message.Photo.First().FileId;
                    _wishListItem.MediaType = MediaTypes.Photo;
                }
                else if (message.Video != null)
                {
                    if (message.Video.Duration > 60)
                    {
                        var warning = await bot.SendMessage(message.Chat.Id, await localization.Get(LocalizationKeys.VideoTooLong, userId), cancellationToken: token);
                        context.MessagesToDelete.Add(message);
                        return false;
                    }
                    _wishListItem.Media = message.Video.FileId;
                    _wishListItem.MediaType = MediaTypes.Video;
                }
                else
                {
                    var warning = await bot.SendMessage(message.Chat.Id, await localization.Get(LocalizationKeys.InvalidMedia, userId), cancellationToken: token);
                    context.MessagesToDelete.Add(message);
                    return false;
                }

                await wishListItemsService.Update(_wishListItem);
                var mes = context.MessageToEdit ?? throw new ArgumentNullException();
                var newText = MessageBuilder.BuildItemMessage(_wishListItem);
                var replyMarkup = await ItemsService.BuildItemMarkup(userId, localization);
                var messagesToDelete = new List<int>();
                messagesToDelete.Add(message.MessageId);
                foreach (var msg in context.MessagesToDelete)
                {
                    messagesToDelete.Add(msg.MessageId);
                }
                if (itemHasMedia)
                {
                    await bot.EditMessageCaption(mes.Chat.Id, mes.Id, newText, replyMarkup: replyMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, cancellationToken: token);
                }
                else
                {
                    messagesToDelete.Add(context.MessageToEdit.MessageId);
                    if (_wishListItem.MediaType == MediaTypes.Photo)
                    {
                        await bot.SendPhoto(mes.Chat.Id, _wishListItem.Media, newText, replyMarkup: replyMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, cancellationToken: token);
                    }
                    else
                    {
                        await bot.SendVideo(mes.Chat.Id, _wishListItem.Media, newText, replyMarkup: replyMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html, cancellationToken: token);
                    }
                }
                await bot.DeleteMessages(message.Chat.Id, messagesToDelete, cancellationToken: token);
                UserContextManager.ClearContext(userId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error SettingMediaState: {ex.Message}");
                return false; // Ensure a return value in case of exception
            }
        }
    }
}
