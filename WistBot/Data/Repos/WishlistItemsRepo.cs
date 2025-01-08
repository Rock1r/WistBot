using Microsoft.EntityFrameworkCore;
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
            return await _context.WishListItems.AsNoTracking().Include(p => p.Photo).Include(v => v.Video).FirstOrDefaultAsync(x => x.Id == id) ?? throw new Exception();
        }

        public async Task<List<WishListItemEntity>> Get()
        {
            return await _context.WishListItems.AsNoTracking().Include(p => p.Photo).Include(v => v.Video).ToListAsync();
        }

        public async Task Add(string name, string description, string link, PhotoSizeEntity? photo, VideoEntity? video, string performerName, State currentState, Guid listId)
        {
            var item = new WishListItemEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Link = link,
                PhotoId = photo?.Id,
                Photo = photo,
                VideoId = video?.Id,
                Video = video,
                PerformerName = performerName,
                CurrentState = currentState,
                ListId = listId
            };
            _context.WishListItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Guid id, string name, string description, string link, PhotoSizeEntity? photo, VideoEntity? video, string performerName, State currentState)
        {
            var photoid = photo?.Id;
            var videoid = video?.Id;
            await _context.WishListItems
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(x =>
                x.SetProperty(n => n.Name, name)
                .SetProperty(n => n.Description, description)
                .SetProperty(n => n.Link, link)
                .SetProperty(n => n.PhotoId, photoid)
                .SetProperty(n => n.Photo, photo)
                .SetProperty(n => n.VideoId, videoid)
                .SetProperty(n => n.Video, video)
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
