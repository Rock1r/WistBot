using WistBot.Enums;

namespace WistBot.Data.Models
{
    public class WishListItemEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public Guid? PhotoId { get; set; }
        public PhotoSizeEntity? Photo { get; set; }
        public Guid? VideoId { get; set; }
        public VideoEntity? Video { get; set; }
        public string PerformerName { get; set; } = string.Empty;
        public State CurrentState { get; set; } = State.Free;
        public Guid ListId { get; set; }
        public WishListEntity? List { get; set; }
    }
}
