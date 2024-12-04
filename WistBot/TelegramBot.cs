using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace WistBot
{
    internal class TelegramBot
    {
        private readonly TelegramBotClient _client;
        private readonly string _token;

        private Localization _localization;
        private enum Commands
        {
            start,
            language,
            help,
            list,
            add,
            remove,
            clear
        }

        public TelegramBot(string token)
        {
            _token = token;
            _client = new TelegramBotClient(token);
            _localization = new Localization("en");
        }
        public void StartReceiving()
        {
            _client.StartReceiving(Update, Error);
        }
        private async Task Update(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            var message = update.Message;
            var callback = update.CallbackQuery;

            if (message != null)
            {
                var chatId = message.Chat.Id;
                if (message.Text != null)
                {
                    if (message.Text.StartsWith("/" + Commands.start))
                    {
                        await bot.SendMessage(chatId, _localization.Get("start_message"));
                    }
                    else
                    if (message.Text.StartsWith("/" + Commands.language))
                    {
                        await bot.SendMessage(chatId, "Choose language",
                            replyMarkup: new InlineKeyboardMarkup().AddButtons(EnglishButton, UkrainianButton));
                    }
                }
                else
                {
                    await bot.SendMessage(chatId, "I can't understand this message.");
                }
            }
            if (callback != null && callback.Message != null)
            {
                var chatId = callback.Message.Chat.Id;
                if (callback.Data != null)
                {
                    if (callback.Data == "en")
                    {
                        _localization = new Localization("en");
                    }
                    else if (callback.Data == "uk")
                    {
                        _localization = new Localization("uk");
                    }
                }
            }
        }

        private Task Error(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private InlineKeyboardButton EnglishButton => new InlineKeyboardButton
        {
            Text = "English",
            CallbackData = "en"
        };

        private InlineKeyboardButton UkrainianButton => new InlineKeyboardButton
        {
            Text = "Ukrainian",
            CallbackData = "uk"
        };
    }
}
