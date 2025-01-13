using System.Text.Json;

namespace WistBot
{
    public class LocalizationService
    {
        private Dictionary<string, string> _texts = new Dictionary<string, string>();
        public LocalizationService(string languageCode)
        {
            string filePath = @$"localization/{languageCode}.json";
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл локалізації для мови {languageCode} не знайдено.");
            }
            string jsonContent = File.ReadAllText(filePath);
            _texts = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
        }
        public string Get(string key)
        {
            return _texts.TryGetValue(key, out var value) ? value : key;
        }

        public void SetLanguage(string languageCode)
        {
            string filePath = @$"localization/{languageCode}.json";
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Файл локалізації для мови {languageCode} не знайдено.");
            }
            string jsonContent = File.ReadAllText(filePath);
            _texts = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
        }
    }
}
