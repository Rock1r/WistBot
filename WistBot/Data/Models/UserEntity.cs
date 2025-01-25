using WistBot.Res;

namespace WistBot.Data.Models
{
    public class UserEntity
    {
        public long TelegramId { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; } = string.Empty;
        public List<WishListEntity> WishLists { get; set; } = [];
        public string Language { get; set; } = LanguageCodes.English;
        public uint MaxListsCount { get; set; } = 20;
    }
}
