using System.Collections.Generic;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Data.Models;
using WistBot.Data.Repos;
using WistBot.Enums;
using WistBot.Res;

namespace WistBot.Services
{
    public class ItemsService
    {
        private readonly ItemsRepo _wishListItemsRepo;

        public ItemsService(ItemsRepo wishListItemsRepo)
        {
            _wishListItemsRepo = wishListItemsRepo;
        }

        public async Task<ItemEntity> GetById(Guid id)
        {
            return await _wishListItemsRepo.GetById(id);
        }

        public async Task<ItemEntity> GetByName(long userId, string name)
        {
            return await _wishListItemsRepo.GetByName(userId, name);
        }

        public async Task<List<ItemEntity>> Get()
        {
            return await _wishListItemsRepo.Get();
        }

        public async Task Add(string name, string description, string link, string media, MediaTypes type, string performerName, State currentState, Guid listId, long userId)
        {
            await _wishListItemsRepo.Add(name, description, link, media, type, performerName, currentState, listId, userId);
        }

        public async Task Add(ItemEntity item)
        {

            await _wishListItemsRepo.Add(item.Name, item.Description, item.Link, item.Media, item.MediaType, item.PerformerName, item.CurrentState, item.ListId, item.OwnerId);
        }

        public async Task Update(Guid id, string name, string description, string link, string media, MediaTypes type, string performerName, State currentState, long userId)
        {
            await _wishListItemsRepo.Update(id, name, description, link, media, type, performerName, currentState, userId);
        }

        public async Task Update(ItemEntity item)
        {
            await _wishListItemsRepo.Update(item.Id, item.Name, item.Description, item.Link, item.Media, item.MediaType, item.PerformerName, item.CurrentState, item.OwnerId);
        }

        public async Task Delete(Guid id)
        {
            await _wishListItemsRepo.Delete(id);
        }

        public async Task Delete(long userId, string name)
        {
            await _wishListItemsRepo.Delete(userId, name);
        }

        public static async Task ViewItem(ITelegramBotClient bot, long chatId, ItemEntity item, InlineKeyboardMarkup markup, CancellationToken token)
        {
            if (!string.IsNullOrWhiteSpace(item.Media))
            {
                var file = await bot.GetFile(item.Media, token);
                if (file != null)
                {
                    if (item.MediaType == MediaTypes.Photo)
                    {
                        await bot.SendPhoto(chatId, item.Media, $"<b>{item.Name}</b>" + "\n" + item.Description + "\n" + item.Link, replyMarkup: markup, cancellationToken: token, parseMode: ParseMode.Html);
                    }
                    else
                    {
                        await bot.SendVideo(chatId, item.Media, $"<b>{item.Name}</b>" + "\n" + item.Description + "\n" + item.Link, replyMarkup: markup, cancellationToken: token, parseMode: ParseMode.Html);
                    }
                }
            }
            else
            {
                await bot.SendMessage(chatId, $"<b>{item.Name}</b>" + "\n" + item.Description + "\n" + item.Link, replyMarkup: markup, cancellationToken: token, parseMode: ParseMode.Html);
            }
        }

        public static async Task ViewAnotherUserItem(ITelegramBotClient bot, long chatId, User sender, ItemEntity item, LocalizationService localizationService, UsersService usersService, CancellationToken token)
        {
            var markup = await ItemsService.BuildUserItemMarkup(sender, localizationService, (usersService.GetById(item.OwnerId).Result.Username), item);
            var name = item.Name;
            var text = await MessageBuilder.BuildUserItemMessage(item, localizationService, sender.Id);
            if (!string.IsNullOrWhiteSpace(item.Media))
            {
                if (item.MediaType == MediaTypes.Photo)
                {
                    await bot.SendPhoto(chatId, item.Media, text, replyMarkup: markup, cancellationToken: token, parseMode: ParseMode.Html);
                }
                else
                {
                    await bot.SendVideo(chatId, item.Media, text, replyMarkup: markup, cancellationToken: token, parseMode: ParseMode.Html);
                }
            }
            else
            {
                await bot.SendMessage(chatId, text, replyMarkup: markup, cancellationToken: token, parseMode: ParseMode.Html);
            }
        }

        public static async Task<InlineKeyboardMarkup> BuildItemMarkup(long userId, LocalizationService localization)
        {
            return new InlineKeyboardMarkup(new[]
                    {
                new[]
                {
                            InlineKeyboardButton.WithCallbackData(await localization.Get(InlineButton.ChangeItemName, userId), BotCallbacks.ChangeItemName),
                            InlineKeyboardButton.WithCallbackData(await localization.Get(InlineButton.SetDescription, userId), BotCallbacks.SetDescription)
                        }, new[]
                        {
                            InlineKeyboardButton.WithCallbackData(await localization.Get(InlineButton.SetMedia, userId), BotCallbacks.SetMedia),
                            InlineKeyboardButton.WithCallbackData(await localization.Get(InlineButton.SetLink, userId), BotCallbacks.SetLink)
                        }, new[]
                        {
                            InlineKeyboardButton.WithCallbackData(await localization.Get(InlineButton.DeleteItem, userId), BotCallbacks.DeleteItem),
                        }
                    });
        }

        public static async Task<IReplyMarkup> BuildUserItemMarkup(User sender, LocalizationService localization, string ownerName, ItemEntity item)
        {
            var senderName = sender.Username ?? sender.FirstName;
            if (!string.IsNullOrWhiteSpace(item.PerformerName))
            {
                if(senderName  == item.PerformerName)
                {
                    var setFreeButton = InlineKeyboardButton.WithCallbackData($"🔵 {await localization.Get(InlineButton.SetFree, sender.Id)}", $"{BotCallbacks.SetFree}:{item.Id}");
                    var setInProcessButton = InlineKeyboardButton.WithCallbackData($"🟡 {await localization.Get(InlineButton.SetInProcess, sender.Id)}", $"{BotCallbacks.SetInProcess}:{item.Id}");
                    var setDoneButton = InlineKeyboardButton.WithCallbackData($"🟢 {await localization.Get(InlineButton.SetDone, sender.Id)}", $"{BotCallbacks.SetDone}:{item.Id}");
                    if (item.CurrentState == State.Free)
                    {
                        return new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                setInProcessButton,
                                setDoneButton
                            }
                        });
                    }
                    else if (item.CurrentState == State.Busy)
                    {
                        return new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                setFreeButton,
                                setDoneButton
                            }
                        });
                    }
                    else
                    {
                        return new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                setFreeButton,
                                setInProcessButton
                            }
                        });
                    }
                }
                else
                {
                    return new ReplyKeyboardRemove();
                }
            }
            else 
            {
                return new InlineKeyboardMarkup(new[]
                    {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(await localization.Get(InlineButton.WantToPresent, sender.Id), $"{BotCallbacks.WantToPresent}:{ownerName}")
                } });
            }
        }
    }
}
