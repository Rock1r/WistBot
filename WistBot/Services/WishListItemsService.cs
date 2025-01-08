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

        public async Task<List<WishListItemEntity>> Get()
        {
            return await _wishListItemsRepo.Get();
        }

        public async Task Add(string name, string description, string link, PhotoSizeEntity? photo, VideoEntity? video, string performerName, State currentState, Guid listId)
        {
            await _wishListItemsRepo.Add(name, description, link, photo, video, performerName, currentState, listId);
        }

        public async Task Add(WishListItemEntity item)
        {
            await _wishListItemsRepo.Add(item.Name, item.Description, item.Link, item.Photo, item.Video, item.PerformerName, item.CurrentState, item.ListId);
        }

        public async Task Update(Guid id, string name, string description, string link, PhotoSizeEntity? photo, VideoEntity? video, string performerName, State currentState)
        {
            await _wishListItemsRepo.Update(id, name, description, link, photo, video, performerName, currentState);
        }

        public async Task Delete(Guid id)
        {
            await _wishListItemsRepo.Delete(id);
        }

    }
}
