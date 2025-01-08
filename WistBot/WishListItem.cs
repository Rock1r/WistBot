using System.Text.Json.Serialization;
using Telegram.Bot.Types;

namespace WistBot
{
    public class WishListItem
    {
        public long Id { get; private set; }
        public string ListName { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Link { get; set; }
        public PhotoSize? Photo { get; set; }
        public Video? Video { get; set; }
        public VideoNote? VideoNote { get; set; }
        public Voice? Voice { get; set; }

        public string? PerformerName { get; set; }
        
        //public State CurrentState { get; set; }

        [JsonConstructor]
        public WishListItem(string listName)
        {
            Id = DateTime.Now.Ticks;
            Name = "Wish";
            ListName = listName;
        }
    }

}
