using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Data.Models;
using WistBot.Data.Repos;

namespace WistBot.Services
{
    public class WishListsService
    {
        private readonly WishlistsRepo _wishListsRepo;

        public WishListsService(WishlistsRepo wishListsRepo)
        {
            _wishListsRepo = wishListsRepo;
        }

        public async Task<WishListEntity> GetById(Guid id)
        {
            return await _wishListsRepo.GetById(id);
        }

        public async Task<List<WishListEntity>> GetByOwnerId(long ownerId)
        {
            return await _wishListsRepo.GetByOwnerId(ownerId);
        }

        public async Task<WishListEntity> GetByName(long ownerId, string name)
        {
            return await _wishListsRepo.GetByName(ownerId, name);
        }

        public async Task<List<WishListEntity>> Get()
        {
            return await _wishListsRepo.Get();
        }

        public async Task<List<WishListEntity>> GetWithItems()
        {
            return await _wishListsRepo.GetWithItems();
        }

        public async Task Add(string name, bool isPublic, long ownerId)
        {
            await _wishListsRepo.Add(name, isPublic, ownerId);
        }

        public async Task Update(Guid id, string name, bool isPublic)
        {
            await _wishListsRepo.Update(id, name, isPublic);
        }

        public async Task Delete(Guid id)
        {
            await _wishListsRepo.Delete(id);
        }

        public async Task Delete(string name)
        {
            await _wishListsRepo.Delete(name);
        }

        public async Task ViewList(ITelegramBotClient _bot, long chatId, long userId, WishListEntity list, LocalizationService _localization, CancellationToken token)
        {
            var culture = await _localization.GetLanguage(userId);
            var keyboard = new ReplyKeyboardMarkup(new KeyboardButton[][]
            {
                 new KeyboardButton[] { await _localization.Get(KButton.AddItem, culture)  },
                new KeyboardButton[] { await _localization.Get(KButton.ClearList, culture) }
                })
            {
                ResizeKeyboard = true
            };
            await _bot.SendMessage(chatId, list.Name, replyMarkup: keyboard, cancellationToken: token);
            if (list.Items.Count > 0)
            {
                foreach (var item in list.Items)
                {
                    var inlineReply = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.ChangeItemName, culture), BotCallbacks.SetName),
                            InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.SetDescription, culture), BotCallbacks.SetDescription)
                        }, new[]
                        {
                            InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.SetMedia, culture), BotCallbacks.SetMedia),
                            InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.SetLink, culture), BotCallbacks.SetLink)
                        }, new[]
                        {
                            InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.DeleteItem, culture), BotCallbacks.DeleteItem),
                        }
                    });
                    if (!string.IsNullOrWhiteSpace(item.Media))
                    {
                        var file = await _bot.GetFile(item.Media);
                        if (file != null)
                        {
                            if (file.FilePath.EndsWith(".jpg") || file.FilePath.EndsWith(".png"))
                            {
                                await _bot.SendPhoto(chatId, item.Media, $"<b>{item.Name}</b>" + "\n" + item.Description + "\n" + item.Link, replyMarkup: inlineReply, cancellationToken: token, parseMode: ParseMode.Html);
                            }
                            else
                            {
                                await _bot.SendVideo(chatId, item.Media, $"<b>{item.Name}</b>" + "\n" + item.Description + "\n" + item.Link, replyMarkup: inlineReply, cancellationToken: token, parseMode: ParseMode.Html);
                            }
                        }
                    }
                    else
                    {
                        await _bot.SendMessage(chatId, $"<b>{item.Name}</b>" + "\n" + item.Description + "\n" + item.Link, replyMarkup: inlineReply, cancellationToken: token, parseMode: ParseMode.Html);
                    }
                }
            }
        }
    }
}
