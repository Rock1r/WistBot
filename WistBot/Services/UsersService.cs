using Telegram.Bot;
using Telegram.Bot.Types;
using WistBot.Data.Models;
using WistBot.Data.Repos;
using WistBot.Exceptions;

namespace WistBot.Services
{
    public class UsersService
    {
        private readonly UsersRepo _usersRepo;
        private readonly Dictionary<long, Guid> _currentList = new();

        public void SetListContext(long userId, Guid listId)
        {
            _currentList[userId] = listId;
        }

        public Guid GetListContext(long userId)
        {
            return _currentList.TryGetValue(userId, out var listId) ? listId : throw new CurrentListNotFoundException(listId);
        }

        public void ClearListContext(long userId)
        {
            _currentList.Remove(userId);
        }

        public UsersService(UsersRepo usersRepo)
        {
            _usersRepo = usersRepo;
        }

        public async Task<bool> UserExists(long telegramId)
        {
            return await _usersRepo.UserExists(telegramId);
        }

        public async Task<string> GetLanguage(long telegramId)
        {
            return await _usersRepo.GetLanguage(telegramId);
        }

        public async Task<UserEntity> GetByUsername(string username)
        {
            return await _usersRepo.GetByUsername(username);
        }

        public async Task<UserEntity> GetById(int userId)
        {
            return await _usersRepo.GetById(userId);
        }

        public async Task<long> GetId(string username)
        {
            return await _usersRepo.GetIdByName(username);
        }

        public async Task<List<UserEntity>> Get()
        {
            return await _usersRepo.Get();
        }

        public async Task<List<UserEntity>> GetWithLists()
        {
            return await _usersRepo.GetWithLists();
        }

        public async Task Add(long telegramId, string username)
        {
            await _usersRepo.Add(telegramId, username);
        }

        public async Task Update(long telegramId, string username)
        {
            await _usersRepo.Update(telegramId, username);
        }

        public async Task Delete(long telegramId)
        {
            await _usersRepo.Delete(telegramId);
        }

        public async Task SetLanguage(long telegramId, string language)
        {
            await _usersRepo.SetLanguage(telegramId, language);
        }

    }
}
