using System.Text.Json.Serialization;
using Telegram.Bot.Types;

namespace WistBot
{
    internal class WishListItem
    {
        public long Id { get; set; }
        public string ListName { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Link { get; set; }
        public PhotoSize? Photo { get; set; }
        public Video? Video { get; set; }
        public VideoNote? VideoNote { get; set; }
        public Voice? Voice { get; set; }
        public long? PerformerId { get; set; }

        public enum State { Free, Busy, Done }
        public State CurrentState { get; set; }

        private static long _nextId = 1;

        [JsonConstructor]
        public WishListItem(string listName)
        {
            Id = _nextId++;
            Name = "Wish";
            ListName = listName;
        }
    }

}
