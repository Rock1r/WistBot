using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using WistBot.Data.Models;
using WistBot.Enums;

namespace WistBot.Data.Repos
{
    public class WishlistItemsRepo
    {
        private readonly WistBotDbContext _context;

        public WishlistItemsRepo(WistBotDbContext context)
        {
            _context = context;
        }

        public async Task<WishListItemEntity> GetById(Guid id)
        {
            return await _context.WishListItems.AsNoTracking().Include(l => l.List).FirstOrDefaultAsync(x => x.Id == id) ?? throw new Exception();
        }

        public async Task<WishListItemEntity> GetByName(string name)
        {
            return await _context.WishListItems.AsNoTracking().Include(l => l.List).FirstOrDefaultAsync(x => x.Name == name) ?? throw new Exception();
        }

        public async Task<List<WishListItemEntity>> Get()
        {
            return await _context.WishListItems.AsNoTracking().Include(l => l.List).ToListAsync();
        }

        public async Task Add(string name, string description, string link, string media, string performerName, State currentState, Guid listId)
        {
            var item = new WishListItemEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Link = link,
                Media = media,
                PerformerName = performerName,
                CurrentState = currentState,
                ListId = listId
            };
            await _context.WishListItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Guid id, string name, string description, string link, string media, string performerName, State currentState)
        {
            await _context.WishListItems
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(x =>
                x.SetProperty(n => n.Name, name)
                .SetProperty(n => n.Description, description)
                .SetProperty(n => n.Link, link)
                .SetProperty(n => n.Media, media)
                .SetProperty(n => n.PerformerName, performerName)
                .SetProperty(n => n.CurrentState, currentState));

            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            await _context.WishListItems
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
        }
    }
}
