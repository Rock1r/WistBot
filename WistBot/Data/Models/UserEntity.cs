namespace WistBot.Data.Models
{
    public class UserEntity
    {
        public long TelegramId { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<WishListEntity> WishLists { get; set; } = [];
        public String Language { get; set; } = LanguageCodes.English;
    }
}
