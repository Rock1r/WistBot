using System.Text.Json.Serialization;
using Telegram.Bot.Types;

namespace WistBot
{
    internal class ListObject
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Link { get; set; }
        public Document? Document { get; set; }
        public PhotoSize? Photo { get; set; }
        public User? Performer { get; set; }

        public enum State { Free, Busy, Done }
        public State CurrentState { get; set; }

        private static long _nextId = 1;

        [JsonConstructor]
        public ListObject()
        {
            Id = _nextId++;
            Name = "Wish";
        }
    }

}
