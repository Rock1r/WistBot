using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WistBot
{
    internal class UserData
    {
        public long TelegramId { get; set; }
        public string Username { get; set; }
        public List<WishList> WishLists { get; set; } = new List<WishList>();

        public UserData()
        {
        }

        public UserData(long telegramId, string username)
        {
            TelegramId = telegramId;
            Username = username;
        }

        public WishList GetWishList(string name)
        {
            var existingList = WishLists.FirstOrDefault(list => list.Name == name);
            if (existingList == null)
            {
                var newList = new WishList { Name = name };
                WishLists.Add(newList);
                return newList;
            }
            return existingList;
        }


        public void UpdateWishList(string name, WishList wishList)
        {
            int index = WishLists.FindIndex(l => l.Name == name);

            if (index == -1)
            {
                WishLists.Add(wishList);
            }
            else
            {
                WishLists[index] = wishList;
            }
        }
    }
}
