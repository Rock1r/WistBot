using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;

namespace WistBot.States
{
    public class SettingDescriptionState : IUserStateHandler
    {
        private readonly WishListItemEntity _wishListItem;

        public SettingDescriptionState(WishListItemEntity wishListItem)
        {
            _wishListItem = wishListItem ?? throw new ArgumentNullException(nameof(wishListItem));
        }

        public async Task HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, WishListItemsService wishListItemsService)
        {
            var description = message.Text;

            if (string.IsNullOrWhiteSpace(description))
            {
                await bot.SendMessage(message.Chat.Id, localization.Get(LocalizationKeys.DescriptionCantBeEmpty), cancellationToken: token);
                return;
            }

            _wishListItem.Description = description;
            await wishListItemsService.Update(_wishListItem);

            await BotActions.ShowList(message, token, localization, await wishListsService.GetById(_wishListItem.ListId));
        }
    }

}
