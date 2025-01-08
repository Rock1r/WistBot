using Telegram.Bot.Types;

namespace WistBot
{
    internal class WishList
    {
        public string Name { get; set; } = string.Empty;
        public List<WishListItem> Items { get; set; } = new List<WishListItem>();
        public bool IsPublic { get; set; } = false;
        public long Id { get; private set; }

        public WishList()
        {
            Id = DateTime.Now.Ticks;
        }
    }
}
