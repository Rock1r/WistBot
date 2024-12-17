using System.Text.Json;
using System.Text.Json.Serialization;

namespace WistBot
{
    internal class Config
    {
        [JsonInclude]
        private readonly string TelegramBotToken;

        [JsonConstructor]
        public Config(string telegramBotToken)
        {
            if (string.IsNullOrWhiteSpace(telegramBotToken))
            {
                throw new ArgumentException("Токен не може бути пустим.", nameof(telegramBotToken));
            }

            TelegramBotToken = telegramBotToken;
            Console.WriteLine("Config loaded.");
        }

        public void UseToken(Action<string> action)
        {
            action?.Invoke(TelegramBotToken);
        }

        public static Config LoadFromJson(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"Файл '{fileName}' не знайдено.");
            }

            string jsonContent = File.ReadAllText(fileName);
            try
            {

                return JsonSerializer.Deserialize<Config>(jsonContent)
                       ?? throw new InvalidOperationException("Не вдалося десеріалізувати конфігурацію.");


            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Помилка у форматі JSON файлу конфігурації.", ex);
            }
        }
    }
}
