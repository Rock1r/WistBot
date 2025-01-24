using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;
using WistBot.Res;
using WistBot.Managers;

namespace WistBot.Core.UserStates
{
    public class AddingNewItemState : IUserStateHandler
    {
        private readonly WishListEntity _wishList;

        public AddingNewItemState(WishListEntity wishList)
        {
            _wishList = wishList ?? throw new ArgumentNullException(nameof(wishList));
        }

        public async Task<bool> HandleStateAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken token, LocalizationService localization, WishListsService wishListsService, ItemsService wishListItemsService)
        {
            try
            {
                if (_wishList.MaxItemsCount >= _wishList.Items.Count)
                {
                    await bot.SendMessage(message.Chat.Id, await localization.Get(LocalizationKeys.MaxItemsCountReached, userId), cancellationToken: token);
                    return true;
                }
                var itemName = message.Text;

                if (string.IsNullOrWhiteSpace(itemName))
                {
                    var warning = await bot.SendMessage(message.Chat.Id, await localization.Get(LocalizationKeys.ListNameCantBeEmpty, userId), cancellationToken: token);
                    var context = UserContextManager.GetContext(userId);
                    context.MessagesToDelete.Add(message);
                    context.MessagesToDelete.Add(warning);
                    return false;
                }

                var baseName = itemName;
                var counter = 1;

                while (_wishList.Items.Any(x => x.Name == itemName))
                {
                    itemName = $"{baseName}{counter}";
                    counter++;
                }

                await wishListItemsService.Add(new ItemEntity { Name = itemName, ListId = _wishList.Id, OwnerId = userId });
                await WishListsService.ViewList(bot, message.Chat.Id, userId, await wishListsService.GetById(_wishList.Id), localization, token);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error AddingNewItemState: {ex.Message}");
                return false; // Ensure a return value in case of exception
            }
        }
    }
}
