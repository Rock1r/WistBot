using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;

namespace WistBot.Core.UserStates
{
    public class SettingMediaState : IUserStateHandler
    {
        private readonly WishListItemEntity _wishListItem;

        public SettingMediaState(WishListItemEntity wishListItem)
        {
            _wishListItem = wishListItem ?? throw new ArgumentNullException(nameof(wishListItem));
        }

        public async Task HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, WishListItemsService wishListItemsService)
        {
            if (message.Document != null)
            {
                await bot.SendMessage(message.Chat.Id, localization.Get(LocalizationKeys.DocumentNotSupported), cancellationToken: token);
                return;
            }

            if (message.Photo != null)
            {
                _wishListItem.Media = message.Photo.First().FileId;
            }
            else if (message.Video != null)
            {
                if (message.Video.Duration > 60)
                {
                    await bot.SendMessage(message.Chat.Id, localization.Get(LocalizationKeys.VideoTooLong), cancellationToken: token);
                    return;
                }

                _wishListItem.Media = message.Video.FileId;
            }
            else
            {
                await bot.SendMessage(message.Chat.Id, localization.Get(LocalizationKeys.InvalidMedia), cancellationToken: token);
                return;
            }

            await wishListItemsService.Update(_wishListItem);
            //await BotActions.ShowList(message, token, localization, await wishListsService.GetById(_wishListItem.ListId));
        }
    }

}
