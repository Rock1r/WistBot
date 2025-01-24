using Telegram.Bot.Types;
using WistBot.Data.Models;
using WistBot.Enums;
using WistBot.Res;
using WistBot.Services;

namespace WistBot
{
    public static class MessageBuilder
    {
        public static string BuildItemMessage(ItemEntity item)
        {
            return $"<b>{item.Name}</b>\n{item.Description}\n{item.Link}";
        }

        public static async Task<string> BuildUserItemMessage(ItemEntity item, LocalizationService localizaitonService, long id)
        {
            var name = item.Name;
            if (item.Link is not null)
            {
                name = "<a href=\"" + item.Link + "\">" + item.Name + "</a>";
            }
            var state = await localizaitonService.Get(LocalizationKeys.FreeItem, id);
            if (item.CurrentState != State.Free)
            {
                state = item.CurrentState == State.Busy ? await localizaitonService.Get(LocalizationKeys.InProcessItem, id, item.PerformerName) : await localizaitonService.Get(LocalizationKeys.DoneItem,  id, item.PerformerName);
            }
            return $"<b>{name}</b>\n{item.Description}\n{item.Link}\n{state}";
        }
    }
}
