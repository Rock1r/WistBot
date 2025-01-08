namespace WistBot.Data.Models
{
    public class PhotoSizeEntity
    {
        public Guid Id { get; set; }
        public string FileId { get; set; } = string.Empty;
        public string FileUniqueId { get; set; } = string.Empty;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public int FileSize { get; set; } = 0;
        public Guid? WishListItemId { get; set; }
        public WishListItemEntity? WishListItem { get; set; }
    }
}
