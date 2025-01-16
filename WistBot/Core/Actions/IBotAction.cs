using Telegram.Bot.Types;

namespace WistBot.Core.Actions
{
    public interface IBotAction
    {
        string Command { get; }
        Task ExecuteMessage(Message message, CancellationToken token);
        Task ExecuteCallback(CallbackQuery callback, CancellationToken token);
    }
}
