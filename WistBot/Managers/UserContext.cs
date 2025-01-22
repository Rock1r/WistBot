using Telegram.Bot.Types;
using WistBot.Data.Models;

namespace WistBot.Managers
{
    public class UserContext
    {
        public ItemEntity? ItemToEdit { get; set; }
        public Message? MessageToEdit { get; set; }
        public List<Message> MessagesToDelete { get; set; } = new List<Message>();

        public UserContext()
        {
        }

        public UserContext(Message messageToDelete)
        {
            MessagesToDelete.Add(messageToDelete);
        }

        public void AddMessageToDelete(Message message)
        {
            MessagesToDelete.Add(message);
        }
    }
}
