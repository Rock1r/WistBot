using WistBot.Data.Models;
using WistBot.Data.Repos;

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

        public async Task<List<WishListEntity>> Get()
        {
            return await _wishListsRepo.Get();
        }

        public async Task<List<WishListEntity>> GetWithItems()
        {
            return await _wishListsRepo.GetWithItems();
        }

        public async Task Add(string name, bool isPublic, long ownerId)
        {
            await _wishListsRepo.Add(name, isPublic, ownerId);
        }

        public async Task Update(Guid id, string name, bool isPublic)
        {
            await _wishListsRepo.Update(id, name, isPublic);
        }

        public async Task Delete(Guid id)
        {
            await _wishListsRepo.Delete(id);
        }
    }
}
