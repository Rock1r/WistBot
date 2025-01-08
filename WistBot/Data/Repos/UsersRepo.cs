using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WistBot.Data.Models;

namespace WistBot.Data.Repos
{
    public class UsersRepo
    {
        private readonly WistBotDbContext _context;

        public UsersRepo(WistBotDbContext context)
        {
            _context = context;
        }

        public async Task<UserEntity> GetById(int userId)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.TelegramId == userId)?? throw new Exception();
        }

        public async Task<List<UserEntity>> Get()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        public async Task<List<UserEntity>> GetWithLists()
        {
            return await _context.Users.AsNoTracking().AsNoTracking().Include(x => x.WishLists).ToListAsync();
        }

        public async Task Add(long telegramId, string username)
        {
            var user = new UserEntity
            {
                TelegramId = telegramId,
                Username = username
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task Update(long telegramId, string username)
        {
            await _context.Users
                .Where(x => x.TelegramId == telegramId)
                .ExecuteUpdateAsync(x =>  
                x.SetProperty(n => n.Username, username));
            await _context.SaveChangesAsync();
        }

        public async Task Delete(long telegramId)
        {
            await _context.Users
                .Where(x => x.TelegramId == telegramId)
                .ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
        }
    }
}
