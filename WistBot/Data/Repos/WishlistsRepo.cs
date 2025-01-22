using Microsoft.EntityFrameworkCore;
using WistBot.Data.Models;
using WistBot.Exceptions;

namespace WistBot.Data.Repos
{
    public class WishlistsRepo
    {
        private readonly WistBotDbContext _context;

        public WishlistsRepo(WistBotDbContext context)
        {
            _context = context;
        }

        public async Task<WishListEntity> GetById(Guid id)
        {
            return await _context.WishLists.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id) ?? throw new ListWithIdNotFoundException(id);
        }

        public async Task<List<WishListEntity>> GetByOwnerId(long ownerId)
        {
            return await _context.WishLists.AsNoTracking().Where(x => x.OwnerId == ownerId).ToListAsync();
        }

        public async Task<WishListEntity> GetByName(long ownerId, string name)
        {
            return await _context.WishLists.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.OwnerId == ownerId && x.Name == name) ?? throw new ListNotFoundException(name);
        }

        public async Task<List<WishListEntity>> Get()
        {
            return await _context.WishLists.AsNoTracking().ToListAsync();
        }

        public async Task<List<WishListEntity>> GetWithItems()
        {
            return await _context.WishLists.AsNoTracking().Include(x => x.Items).ToListAsync();
        }

        public async Task<List<WishListEntity>> GetPublic(long ownerId)
        {
            return await _context.WishLists.AsNoTracking().Where(x => x.OwnerId == ownerId && x.IsPublic).ToListAsync() ?? new List<WishListEntity> { };
        }

        public async Task Add(string name, bool isPublic, long ownerId)
        {
            var wishlist = new WishListEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                IsPublic = isPublic,
                OwnerId = ownerId
            };
            _context.WishLists.Add(wishlist);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Guid id, string name, bool isPublic)
        {
            await _context.WishLists
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(x =>
                x.SetProperty(n => n.Name, name)
                .SetProperty(n => n.IsPublic, isPublic));
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            await _context.WishLists
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
        }

        public async Task Delete(string name)
        {
            await _context.WishLists
                .Where(x => x.Name == name)
                .ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
        }
    }
}
