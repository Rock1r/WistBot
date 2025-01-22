using Microsoft.EntityFrameworkCore;
using WistBot.Data.Models;
using WistBot.Enums;
using WistBot.Exceptions;

namespace WistBot.Data.Repos
{
    public class ItemsRepo
    {
        private readonly WistBotDbContext _context;

        public ItemsRepo(WistBotDbContext context)
        {
            _context = context;
        }

        public async Task<ItemEntity> GetById(Guid id)
        {
            return await _context.Items.AsNoTracking().Include(l => l.List).FirstOrDefaultAsync(x => x.Id == id) ?? throw new ItemWithIdNotFoundException(id);
        }

        public async Task<ItemEntity> GetByName(long userId, string name)
        {
            return await _context.Items.AsNoTracking().Include(l => l.List).FirstOrDefaultAsync(x => x.OwnerId == userId && x.Name == name) ?? throw new ItemNotFoundException(name);
        }

        public async Task<List<ItemEntity>> Get()
        {
            return await _context.Items.AsNoTracking().Include(l => l.List).ToListAsync();
        }

        public async Task Add(string name, string description, string link, string media, string performerName, State currentState, Guid listId, long userId)
        {
            var item = new ItemEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Link = link,
                Media = media,
                PerformerName = performerName,
                CurrentState = currentState,
                ListId = listId,
                OwnerId = userId
            };
            await _context.Items.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Guid id, string name, string description, string link, string media, string performerName, State currentState, long userId)
        {
            await _context.Items
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(x =>
                x.SetProperty(n => n.Name, name)
                .SetProperty(n => n.Description, description)
                .SetProperty(n => n.Link, link)
                .SetProperty(n => n.Media, media)
                .SetProperty(n => n.PerformerName, performerName)
                .SetProperty(n => n.CurrentState, currentState)
                .SetProperty(n => n.OwnerId, userId));

            await _context.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            await _context.Items
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
        }

        public async Task Delete(long userId, string name )
        {
            await _context.Items
                .Where(x => x.OwnerId == userId && x.Name == name)
                .ExecuteDeleteAsync();
            await _context.SaveChangesAsync();

        }
    }
}
