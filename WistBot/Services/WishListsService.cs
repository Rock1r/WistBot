using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Data.Models;
using WistBot.Data.Repos;
using WistBot.Res;

namespace WistBot.Services
{
    public class WishListsService
    {
        private readonly WishlistsRepo _wishListsRepo;

        public WishListsService(WishlistsRepo wishListsRepo)
        {
            _wishListsRepo = wishListsRepo;
        }

        public async Task<WishListEntity> GetById(Guid id)
        {
            return await _wishListsRepo.GetById(id);
        }

        public async Task<List<WishListEntity>> GetByOwnerId(long ownerId)
        {
            return await _wishListsRepo.GetByOwnerId(ownerId);
        }

        public async Task<WishListEntity> GetByName(long ownerId, string name)
        {
            return await _wishListsRepo.GetByName(ownerId, name);
        }

        public async Task<List<WishListEntity>> Get()
        {
            return await _wishListsRepo.Get();
        }

        public async Task<List<WishListEntity>> GetWithItems()
        {
            return await _wishListsRepo.GetWithItems();
        }

        public async Task<List<WishListEntity>> GetPublic(long ownerId)
        {
            return await _wishListsRepo.GetPublic(ownerId);
        }

        public async Task Add(string name, long ownerId)
        {
            await _wishListsRepo.Add(name, ownerId);
        }

        public async Task Update(Guid id, string name, bool isPublic)
        {
            await _wishListsRepo.Update(id, name, isPublic);
        }

        public async Task Delete(Guid id)
        {
            await _wishListsRepo.Delete(id);
        }

        public async Task Delete(long userId, string name)
        {
            await _wishListsRepo.Delete(userId, name);
        }

        public static async Task<InlineKeyboardMarkup> GetListMarkup(WishListEntity list, LocalizationService localizationService)
        {
            var visibilityButtonText = await localizationService.Get(InlineButton.ChangeVisіbility, list.OwnerId, list.IsPublic ?
                        await localizationService.Get(LocalizationKeys.MakePrivate, list.OwnerId) :
                        await localizationService.Get(LocalizationKeys.MakePublic, list.OwnerId));
            return new InlineKeyboardMarkup(new[]
            {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await localizationService.Get(InlineButton.WatchList, list.OwnerId), BotCallbacks.List)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await localizationService.Get(InlineButton.ChangeListName, list.OwnerId), BotCallbacks.ChangeListName),
                        InlineKeyboardButton.WithCallbackData(visibilityButtonText, BotCallbacks.ChangeVisіbility)
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await localizationService.Get(InlineButton.DeleteList, list.OwnerId), BotCallbacks.DeleteList),
                        //InlineKeyboardButton.WithCallbackData(await _localization.Get(Button.ShareList, list.OwnerId), BotCallbacks.ShareList)
                    }
                    
                });
        }

        public static async Task ViewList(ITelegramBotClient _bot, long chatId, long userId, WishListEntity list, LocalizationService _localization, CancellationToken token)
        {
            var culture = await _localization.GetLanguage(userId);
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.AddItem, userId), BotCallbacks.AddItem)
                },
            });
            var mes = await _bot.SendMessage(chatId, list.Name, replyMarkup: keyboard, cancellationToken: token);
            if (list.Items.Count > 0)
            {
                foreach (var item in list.Items)
                {
                    await ItemsService.BuildItemMarkup(userId, _localization);
                    await ItemsService.ViewItem(_bot, chatId, item, await ItemsService.BuildItemMarkup(userId, _localization), token);
                }
            }
        }

        public static async Task ViewUserList(ITelegramBotClient _bot, long chatId, User sender, WishListEntity list, LocalizationService _localization, UsersService usersService, CancellationToken token)
        {
            try
            {

                if (list.Items.Count > 0)
                {
                    foreach (var item in list.Items)
                    {
                        await ItemsService.ViewAnotherUserItem(_bot, chatId, sender, item, _localization, usersService, token);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ViewUserList: {ex.Message}");
            }
        }
    }
}
