using System.Globalization;
using System.Resources;
using WistBot.Exceptions;
using WistBot.Res;

namespace WistBot.Services
{
    public class LocalizationService
    {
        private readonly UsersService _usersService;
        private readonly ResourceManager _resourceManager;

        public string[] AvailableLanguages => new[] { LanguageCodes.English, LanguageCodes.Ukrainian };
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

        public async Task SwitchLanguage(long userId)
        {
            var currentLanguage = await _usersService.GetLanguage(userId);
            var newLanguage = currentLanguage == LanguageCodes.English ? LanguageCodes.Ukrainian : LanguageCodes.English;
            await _usersService.SetLanguage(userId, newLanguage);
        }

        public async Task<CultureInfo> GetLanguage(long userId)
        {
            return new CultureInfo(await _usersService.GetLanguage(userId));
        }

        public string Get(string key)
        {
            return _resourceManager.GetString(key) ?? key;
        }

        public async Task<string> Get(string key, long userId)
        {
            return await Get(key, await GetLanguage(userId));
        }

        public async Task<string> Get(string key, long userId, params object[] args)
        {
            var cultureInfo = new CultureInfo(await _usersService.GetLanguage(userId));
            return await Get(key, cultureInfo, args);
        }

        public async Task<string> Get(string key, CultureInfo cultureInfo, params object[] args)
        {
            return await Task.Run(() =>
            {
                var resourceValue = _resourceManager.GetString(key, cultureInfo);
                if (resourceValue == null)
                {
                    throw new LocalizedStringNotFoundException(key);
                }
                return string.Format(resourceValue, args);
            });
        }
    }
}
