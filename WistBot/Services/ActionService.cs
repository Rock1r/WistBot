using System.Globalization;
using Telegram.Bot.Types;
using WistBot.Core.Actions;

namespace WistBot.Services
{
    public class ActionService
    {
        private readonly Dictionary<string, IBotAction> _localizedActions;

        private ActionService(Dictionary<string, IBotAction> localizedActions)
        {
            _localizedActions = localizedActions;
        }

        public static async Task<ActionService> CreateAsync(
            IEnumerable<IBotAction> actions,
            LocalizationService localizationService)
        {
            var localizedActions = new Dictionary<string, IBotAction>();

            foreach (var action in actions)
            {
                if (action.Command.Contains("callback"))
                {
                    if (!localizedActions.TryAdd(action.Command, action))
                    {
                        throw new InvalidOperationException($"Callback command '{action.Command}' is already registered.");
                    }
                }
                else if (action.Command.StartsWith("/"))
                {
                    if (!localizedActions.TryAdd(action.Command, action))
                    {
                        throw new InvalidOperationException($"Command '{action.Command}' is already registered.");
                    }
                }
                else
                {
                    foreach (var language in localizationService.AvailableLanguages)
                    {
                        var localizedCommand = await localizationService.Get(action.Command, new CultureInfo(language));
                        if (!localizedActions.TryAdd(localizedCommand, action))
                        {
                            throw new InvalidOperationException($"Command '{localizedCommand}' is already registered.");
                        }
                    }
                }
            }


            return new ActionService(localizedActions);
        }

        public async Task ExecuteMessage(string command, Message message, CancellationToken token)
        {
            if (_localizedActions.TryGetValue(command, out var action))
            {
                await action.ExecuteMessage(message, token);
            }
            else if (command.StartsWith("@"))  // Якщо текст починається з @
            {
                var findUserAction = _localizedActions["/finduser"];
                await findUserAction.ExecuteMessage(message, token);
            }
            else
            {
                throw new KeyNotFoundException($"No action found for command '{command}'.");
            }
        }

        public async Task ExecuteCallback(string command, CallbackQuery callback, CancellationToken token)
        {
            if (_localizedActions.TryGetValue(command, out var action))
            {
                await action.ExecuteCallback(callback, token);
            }
            else if (command.Contains(':'))
            {
                var actionName = command.Split(':')[0];
                if (_localizedActions.TryGetValue(actionName, out var act))
                {
                    await act.ExecuteCallback(callback, token);
                }
                else
                {
                    throw new KeyNotFoundException($"No action found for command '{command}'.");
                }
            }
            else
            {
                throw new KeyNotFoundException($"No action found for command '{command}'.");
            }
        }
    }
}
