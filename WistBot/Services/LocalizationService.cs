using System.Globalization;
using System.Resources;
using WistBot.Exceptions;

namespace WistBot.Services
{
    public class LocalizationService
    {
        private readonly UsersService _usersService;
        private readonly ResourceManager _resourceManager;

        public LocalizationService(UsersService usersService, ResourceManager resourceManager)
        {
            _usersService = usersService;
            _resourceManager = resourceManager;
        }

        public async Task SetLanguage(long userId, string language)
        {
            if (!string.IsNullOrEmpty(language))
            {
                await _usersService.SetLanguage(userId, language);
            }
        }

        public async Task<string> GetLanguage(long userId)
        {
            return await _usersService.GetLanguage(userId);
        }

        public string Get(string key)
        {
            return _resourceManager.GetString(key) ?? key;
        }

        public async Task<string> Get(string key, long userId)
        {
            var language = await GetLanguage(userId);
            var cultureInfo = new CultureInfo(language);
            return Get(key, cultureInfo);
        }

        public async Task<string> Get(string key, long userId, params object[] args)
        {
            var cultureInfo = new CultureInfo(await _usersService.GetLanguage(userId));
            return Get(key, cultureInfo, args);
        }

        private string Get(string key, CultureInfo cultureInfo, params object[] args)
        {
            return string.Format(_resourceManager.GetString(key, cultureInfo) ?? throw new LocalizedStringNotFoundException(key), args);
        }
    }
}
