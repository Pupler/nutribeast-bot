using NutriBeastBot.Keyboards;
using NutriBeastBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NutriBeastBot.Handlers;

public partial class UpdateHandler
{
    public async Task HandleCallbackAsync(
        ITelegramBotClient bot,
        Update update,
        CancellationToken ct
    )
    {
        if (update.CallbackQuery == null)
        {
            return;
        }

        var data = update.CallbackQuery.Data;
        var chatId = update.CallbackQuery.Message!.Chat.Id;
        var messageId = update.CallbackQuery.Message!.MessageId;

        switch(data)
        {
            case "main_menu":
                userStateService.SetState(chatId, UserState.Idle);
                await bot.DeleteMessage(
                    chatId,
                    messageId,
                    cancellationToken: ct
                );

                await HandleIdle(bot, chatId, command: "/start", ct);
                break;
            case "menu_food":
                await HandleFoodMenu(bot, chatId, messageId, ct);
                break;
            case "menu_stats":
                await HandleStatsMenu(bot, chatId, messageId, ct);
                break;
            case "goal_menu":
                await HandleGoalMenu(bot, chatId, messageId, ct);
                break;
            case "menu_settings":
                await HandleSettingsMenu(bot, chatId, messageId, ct);
                break;
            case "settings_reminders":
                await HandleReminderMenu(bot, chatId, messageId, ct);
                break;
            case "settings_language":
                await HandleLanguageMenu(bot, chatId, messageId, ct);
                break;
            case "goal_view":
                await HandleGoalView(bot, chatId, messageId, ct);
                break;
            case "goal_delete":
                await HandleDeleteGoal(bot, chatId, messageId, ct);
                break;
            case "add_food":
                await HandleAddFood(bot, chatId, messageId, ct);
                break;
            case "food_edit":
                await HandleFoodEdit(bot, chatId, messageId, ct);
                break;
            case "check_today": 
                await HandleCheckToday(bot, chatId, messageId, ct);
                break;
            case "check_history": 
                await HandleCheckHistory(bot, chatId, messageId, ct);
                break;
            case "food_confirm_add":
                await HandleFoodConfirm(bot, chatId, messageId, ct);
                break;
            case "manage_goal":
                await HandleUpdateGoal(bot, chatId, messageId, ct);
                break;
            case "set_goal":
                await HandleSetGoal(bot, chatId, messageId, ct);
                break;
            case "cancel":
                await HandleCancel(bot, chatId, messageId, ct);
                break;
            default:
                if (data!.StartsWith("goal_gender_"))
                {
                    await HandleGenderSelected(bot, chatId, data, ct);
                }

                else if (data!.StartsWith("aim_"))
                {
                    await HandleAimSelected(bot, chatId, data, ct);
                }

                else if (data!.StartsWith("history_"))
                {
                    await HandleHistoryDate(bot, chatId, data, messageId, ct);
                }

                else if (data!.StartsWith("edit_"))
                {
                    await HandleMacroEdit(bot, chatId, data, messageId, ct);
                }

                else if (data!.StartsWith("reminder_"))
                {
                    await HandleReminder(bot, chatId, data, messageId, ct);
                }
                break;
        }
    }

    private async Task HandleReminder(
        ITelegramBotClient bot,
        long chatId,
        string data,
        int messageId,
        CancellationToken ct
    )
    {
        var reminderData = data.Replace("reminder_", "");

        switch (reminderData)
        {
            case "toggle":
                await databaseService.ToggleReminderAsync(chatId);
                break;
            default:
                break;
        }
    }

    private async Task HandleCancel(
        ITelegramBotClient bot,
        long chatId,
        int messageId,
        CancellationToken ct
    )
    {
        await bot.DeleteMessage(
            chatId,
            messageId,
            cancellationToken: ct
        );

        await bot.SendMessage(
            chatId,
            text: "❌ *Cancelled!*\n\n No worries, whenever you're ready 😊",
            cancellationToken: ct,
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.BackToMainMenu()
        );

        userStateService.SetState(chatId, UserState.Idle);
    }

    private static async Task HandleSettingsMenu(
        ITelegramBotClient bot,
        long chatId,
        int messageId,
        CancellationToken ct
    )
    {
        await bot.DeleteMessage(
            chatId,
            messageId,
            cancellationToken: ct
        );

        await bot.SendMessage(
            chatId,
            text: "⚙️ *Settings*\n\nWhat would you like to configure?",
            cancellationToken: ct,
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.SettingsMenu()
        );
    }

    private static async Task HandleLanguageMenu(
        ITelegramBotClient bot,
        long chatId,
        int messageId,
        CancellationToken ct
    )
    {
        await bot.DeleteMessage(
            chatId,
            messageId,
            cancellationToken: ct
        );

        await bot.SendMessage(
            chatId,
            text: "🌍 *Language*\n\nChoose your language:",
            cancellationToken: ct,
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.LanguageMenu()
        );
    }

    private async Task HandleReminderMenu(
        ITelegramBotClient bot,
        long chatId,
        int messageId,
        CancellationToken ct
    )
    {
        var isEnabled = await databaseService.IsReminderEnabledAsync(chatId);
        
        await bot.DeleteMessage(
            chatId,
            messageId,
            cancellationToken: ct
        );

        await bot.SendMessage(
            chatId,
            text: "🔔 *Reminders*\n\nChoose your daily reminder time:",
            cancellationToken: ct,
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.ReminderMenu(isEnabled)
        );
    }
}