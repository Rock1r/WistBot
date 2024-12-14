using Telegram.Bot.Types;

namespace WistBot
{
    internal class WishList : List<ListObject>
    {
        public readonly User Owner;

        public WishList(User owner) : base()
        {
            Owner = owner;            
        }
    }
}
