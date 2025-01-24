using Microsoft.EntityFrameworkCore;
using WistBot.Data.Models;
using WistBot.Res;

namespace WistBot.Data.Repos
{
    public class UsersRepo
    {
        private readonly WistBotDbContext _context;

        public UsersRepo(WistBotDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UserExists(long id)
        {
            return await _context.Users.AsNoTracking().AnyAsync(x => x.TelegramId == id);
        }

        public async Task<bool> UserExists(string name)
        {
            return await _context.Users.AsNoTracking().AnyAsync(x => x.Username == name);
        }

        public async Task<string> GetLanguage(long telegramId)
        {
            return await _context.Users.AsNoTracking().Where(x => x.TelegramId == telegramId).Select(x => x.Language).FirstOrDefaultAsync() ?? LanguageCodes.English;
        }

        public async Task<UserEntity> GetById(long userId)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.TelegramId == userId)?? throw new Exception();
        }

        public async Task<UserEntity> GetByUsername(string username)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username) ?? throw new Exception();
        }

        public async Task<long> GetIdByName(string username)
        {
            return await _context.Users.AsNoTracking().Where(x => x.Username == username).Select(x => x.TelegramId).FirstOrDefaultAsync();
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

        public async Task SetLanguage(long telegramId, string language)
        {
            await _context.Users
                .Where(x => x.TelegramId == telegramId)
                .ExecuteUpdateAsync(x =>
                x.SetProperty(n => n.Language, language));
            await _context.SaveChangesAsync();
        }

        public async Task SetListsCount(long telegramId, uint count)
        {
            await _context.Users
                .Where(x => x.TelegramId == telegramId)
                .ExecuteUpdateAsync(x =>
                x.SetProperty(n => n.MaxListsCount, count));
            await _context.SaveChangesAsync();
        }
    }
}
