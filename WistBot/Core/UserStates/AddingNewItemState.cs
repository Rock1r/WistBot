using Telegram.Bot.Types;
using Telegram.Bot;
using WistBot.Data.Models;
using WistBot.Services;
using WistBot.Res;
using WistBot.Managers;
using Serilog;

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
                if (_wishList.Items.Count >= _wishList.MaxItemsCount)
                {
                    await bot.SendMessage(message.Chat.Id, await localization.Get(LocalizationKeys.MaxItemsCountReached, userId), cancellationToken: token);
                    Log.Information($"User {userId} tried to add new item to list {_wishList.Name} but max items count reached");
                    return true;
                }
                var itemName = message.Text;

                var context = UserContextManager.GetContext(userId);
                if (string.IsNullOrWhiteSpace(itemName))
                {
                    var warning = await bot.SendMessage(message.Chat.Id, await localization.Get(LocalizationKeys.ListNameCantBeEmpty, userId), cancellationToken: token);
                    context.MessagesToDelete.Add(message);
                    context.MessagesToDelete.Add(warning);
                    return false;
                }
                context.MessagesToDelete.Add(message);

                var baseName = itemName;
                var counter = 1;

                while (_wishList.Items.Any(x => x.Name == itemName))
                {
                    itemName = $"{baseName}{counter}";
                    counter++;
                }
                var item = new ItemEntity { Name = itemName, ListId = _wishList.Id, OwnerId = userId };
                await wishListItemsService.Add(item);
                await ItemsService.ViewItem(bot, message.Chat.Id, item, await ItemsService.BuildItemMarkup(userId, localization), token);
                await UserContextManager.DeleteMessages(bot, userId, message.Chat.Id, context, token);

                Log.Information($"User {userId} added new item {itemName} to list {_wishList.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddingNewItemState");
                return false; // Ensure a return value in case of exception
            }
        }
    }
}
