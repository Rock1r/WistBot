namespace WistBot.Data.Models
{
    public class WishListEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = false;
        public List<ItemEntity> Items { get; set; } = [];
        public long OwnerId { get; set; }
        public UserEntity? Owner { get; set; }
    }
}
