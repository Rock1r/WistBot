﻿using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WistBot.Core.UserStates;
using WistBot.Exceptions;
using WistBot.Managers;
using WistBot.Res;
using WistBot.Services;

namespace WistBot.Core.Actions
{
    public class SetLinkCallbackAction : IBotAction
    {
        private readonly ITelegramBotClient _bot;
        private readonly LocalizationService _localization;
        private readonly UserStateManager _userStateManager;
        private readonly ItemsService _wishListItemsService;

        public string Command => BotCallbacks.SetLink;

        public SetLinkCallbackAction(ITelegramBotClient bot, LocalizationService localizationService, ItemsService wishListItemsService, UserStateManager userStateManager)
        {
            _bot = bot;
            _localization = localizationService;
            _wishListItemsService = wishListItemsService;
            _userStateManager = userStateManager;
        }

        public Task ExecuteMessage(Message message, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteCallback(CallbackQuery callback, CancellationToken token)
        {
            try
            {
                if (callback == null)
                {
                    return;
                }
                var message = callback.Message ?? throw new ArgumentNullException(nameof(callback.Message));
                var chatId = message.Chat.Id;
                var userId = callback.From.Id;
                var text = message.Text ?? message.Caption ?? throw new ArgumentNullException(nameof(message.Text));
                text = text.Split("\n")[0];
                var item = await _wishListItemsService.GetByName(userId, text);
                _userStateManager.SetState(userId, new SettingLinkState(item));
                var inlineReply = new InlineKeyboardMarkup().AddButton(await _localization.Get(InlineButton.Cancel, userId), BotCallbacks.Cancel);
                var messageToSend = string.Empty;
                if (!string.IsNullOrEmpty(item.Link))
                {
                    inlineReply.AddButton(InlineKeyboardButton.WithCallbackData(await _localization.Get(InlineButton.DeleteLink, userId), BotCallbacks.DeleteLink));
                    messageToSend = await _localization.Get(LocalizationKeys.SetOrDeleteLink, userId);
                }
                else
                {
                    messageToSend = await _localization.Get(LocalizationKeys.SetLink, userId);
                }

                var mes = await _bot.SendMessage(chatId, messageToSend, replyMarkup: inlineReply, cancellationToken: token);
                UserContextManager.SetContext(userId, new UserContext(mes) { ItemToEdit = item, MessageToEdit = message });
            }
            catch (ItemNotFoundException ex)
            {
                await _bot.AnswerCallbackQuery(callback.Id, ex.Message, cancellationToken: token);
                Log.Error(ex, "Error SetLinkCallbackAction, item not found");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error SetLinkCallbackAction");
            }
        }
    }
}
