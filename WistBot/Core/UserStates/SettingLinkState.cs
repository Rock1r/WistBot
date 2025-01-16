using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;

namespace WistBot.Core.UserStates
{
    public class SettingLinkState : IUserStateHandler
    {
        private readonly WishListItemEntity _wishListItem;

        public SettingLinkState(WishListItemEntity wishListItem)
        {
            _wishListItem = wishListItem ?? throw new ArgumentNullException(nameof(wishListItem));
        }

        public async Task HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, WishListItemsService wishListItemsService)
        {
            var link = message.Text;

            if (!Uri.IsWellFormedUriString(link, UriKind.Absolute))
            {
                await bot.SendMessage(message.Chat.Id, localization.Get(LocalizationKeys.InvalidLink), cancellationToken: token);
                return;
            }

            _wishListItem.Link = link;
            await wishListItemsService.Update(_wishListItem);

            await BotActions.ShowList(message, token, localization, await wishListsService.GetById(_wishListItem.ListId));
        }
    }

}
