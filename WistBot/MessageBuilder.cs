using WistBot.Data.Models;
using WistBot.Enums;

namespace WistBot
{
    public static class MessageBuilder
    {
        public static string BuildItemMessage(ItemEntity item)
        {
            return $"<b>{item.Name}</b>\n{item.Description}\n{item.Link}";
        }

        public static string BuildUserItemMessage(ItemEntity item)
        {
            var name = item.Name;
            if (item.Link is not null)
            {
                name = "<a href=\"" + item.Link + "\">" + item.Name + "</a>";
            }
            var state = "Free ";
            if (item.CurrentState != State.Free)
            {
                state = item.CurrentState == State.Busy ? "In process by @" : "Done by @";
            }
            return $"<b>{name}</b>\n{item.Description}\n{item.Link}\n{state}{item.PerformerName}";
        }
    }
}
