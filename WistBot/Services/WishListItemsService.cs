using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Data.Models;
using WistBot.Data.Repos;
using WistBot.Enums;

namespace WistBot.Services
{
    public class WishListItemsService
    {
        private readonly WishlistItemsRepo _wishListItemsRepo;

        public WishListItemsService(WishlistItemsRepo wishListItemsRepo)
        {
            _wishListItemsRepo = wishListItemsRepo;
        }

        public async Task<WishListItemEntity> GetById(Guid id)
        {
            return await _wishListItemsRepo.GetById(id);
        }

        public async Task<WishListItemEntity> GetByName(string name)
        {
            return await _wishListItemsRepo.GetByName(name);
        }

        public async Task<List<WishListItemEntity>> Get()
        {
            return await _wishListItemsRepo.Get();
        }

        public async Task Add(string name, string description, string link, PhotoSize? photo, Video? video, string performerName, State currentState, Guid listId)
        {
            var media = photo != null ? photo.FileId : video != null ? video.FileId : string.Empty;
           
            await _wishListItemsRepo.Add(name, description, link, media, performerName, currentState, listId);
        }

        public async Task Add(WishListItemEntity item)
        {

            await _wishListItemsRepo.Add(item.Name, item.Description, item.Link, item.Media, item.PerformerName, item.CurrentState, item.ListId);
        }

        public async Task Update(Guid id, string name, string description, string link, PhotoSize? photo, Video? video, string performerName, State currentState)
        {
            var media = photo != null ? photo.FileId : video != null ? video.FileId : string.Empty;
            await _wishListItemsRepo.Update(id, name, description, link, media, performerName, currentState);
        }

        public async Task Update(WishListItemEntity item)
        {
            await _wishListItemsRepo.Update(item.Id, item.Name, item.Description, item.Link, item.Media, item.PerformerName, item.CurrentState);
        }

        public async Task Delete(Guid id)
        {
            await _wishListItemsRepo.Delete(id);
        }

        public async Task ViewItem(ITelegramBotClient bot, long userId, string itemName, LocalizationService localization, CancellationToken token)
        {

        }
    }
}
