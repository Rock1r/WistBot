using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;
using WistBot.Core.Actions;

namespace WistBot.Core.UserStates
{
    public class SettingListNameState : IUserStateHandler
    {
        private readonly WishListEntity _wishList;

        public SettingListNameState(WishListEntity wishList)
        {
            _wishList = wishList;
        }

        public async Task HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, WishListItemsService wishListItemsService)
        {
            var newListName = message.Text;

            if (string.IsNullOrWhiteSpace(newListName))
            {
                await bot.SendMessage(message.Chat.Id, localization.Get(LocalizationKeys.NameCantBeEmpty), cancellationToken: token);
                return;
            }
            var counter = 0;
            var lists = await wishListsService.Get();
            while (lists.Any(x => x.Name == newListName))
            {
                counter++;
                newListName = $"{message.Text} ({counter})";
            }

            if (_wishList != null)
            {
                await wishListsService.Update(_wishList.Id, newListName, _wishList.IsPublic);
            }
            else
            {
                await wishListsService.Add(newListName, false, userId);
            }
            await new ListsAction(bot, wishListsService, localization).ExecuteMessage(message, token);
        }
    }
}
