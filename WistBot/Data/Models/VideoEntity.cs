namespace WistBot.Data.Models
{
    public class VideoEntity
    {
        public Guid Id { get; set; }
        public string FileId { get; set; } = string.Empty;
        public string FileUniqueId { get; set; } = string.Empty;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public int Duration { get; set; } = 0;
        public PhotoSizeEntity? Thumb { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public int FileSize { get; set; } = 0;
        public Guid? WishListItemId { get; set; }
        public WishListItemEntity? WishListItem { get; set; }
    }
}
