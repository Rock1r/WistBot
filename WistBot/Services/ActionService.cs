using Telegram.Bot.Types;
using WistBot.Core.Actions;

namespace WistBot.Services
{
    public class ActionService
    {
        private readonly Dictionary<string, IBotAction> _actions = new();

        public ActionService(IEnumerable<IBotAction> actions)
        {
           
            foreach (var action in actions)
            {
                if (!_actions.TryAdd(action.Command, action))
                {
                    throw new InvalidOperationException($"Command '{action.Command}' is already registered.");
                }
            }
        }

        public async Task ExecuteMessage(string command, Message message, CancellationToken token)
        {
            if (_actions.TryGetValue(command, out var action))
            {
                await action.ExecuteMessage(message, token);
            }
            else
            {
                throw new KeyNotFoundException($"No action found for command '{command}'.");
            }
        }

        public async Task ExecuteCallback(string command, CallbackQuery callback, CancellationToken token)
        {
            if (_actions.TryGetValue(command, out var action))
            {
                await action.ExecuteCallback(callback, token);
            }
            else
            {
                throw new KeyNotFoundException($"No action found for command '{command}'.");
            }
        }
    }
}
