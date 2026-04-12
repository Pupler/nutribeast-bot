using NutriBeastBot.Extensions;
using NutriBeastBot.Keyboards;
using NutriBeastBot.Models;
using NutriBeastBot.Services;
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
            case "food_cancel":
                await HandleFoodCancel(bot, chatId, messageId, ct);
                break;
            case "manage_goal":
                await HandleManageGoal(bot, chatId, ct);
                break;
            case "set_goal":
                await HandleSetGoal(bot, chatId, ct);
                break;
            case "cancel_goal":
                await HandleCancelGoal(bot, chatId, messageId, ct);
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
                break;
        }
    }

    private async Task HandleAddFood(
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
            text: "*Write your food name and grams:*\n(Format - `chicken breast 200g`)",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct,
            replyMarkup: BotKeyboards.BackToMainMenu()
        );
        
        userStateService.SetState(chatId, UserState.WaitingFoodName);
        logger.LogInformation("User {ChatId} state: {State}", chatId, userStateService.GetState(chatId));
    }

    private async Task HandleCheckToday(
        ITelegramBotClient bot,
        long chatId,
        int messageId,
        CancellationToken ct
    )
    {
        var todayLog = await databaseService.GetTodayLogsAsync(chatId);

        var totalKcal = todayLog.Sum(l => l.Calories).Round();
        var totalProtein = todayLog.Sum(l => l.Protein).Round();
        var totalFat = todayLog.Sum(l => l.Fat).Round();
        var totalCarbs = todayLog.Sum(l => l.Carbs).Round();
        var totalSugar = todayLog.Sum(l => l.Sugar).Round();

        var getGoal = await databaseService.GetGoal(chatId);

        await bot.DeleteMessage(
            chatId,
            messageId,
            cancellationToken: ct
        );

        await bot.SendMessage(
            chatId,
            text: $"*📊 Today's summary*\n\n🔥 Calories: {totalKcal} / {getGoal?.Calories} kcal\n🥩 Protein: {totalProtein} / {getGoal?.Protein}g\n🧈 Fat: {totalFat} / {getGoal?.Fat}g\n🍞 Carbs: {totalCarbs} (sugar: {totalSugar}g) / {getGoal?.Carbs}g",
            cancellationToken: ct,
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.BackToMainMenu()
        );
    }

    private async Task HandleCheckHistory(
        ITelegramBotClient bot,
        long chatId,
        int messageId,
        CancellationToken ct
    )
    {
        var days = await databaseService.GetDaysWithLogsAsync(chatId);

        await bot.DeleteMessage(
            chatId,
            messageId,
            cancellationToken: ct
        );
        
        if (!days.Any())
        {
            await bot.SendMessage(
                chatId,
                text: "No history yet 📭",
                cancellationToken: ct,
                replyMarkup: BotKeyboards.BackToMainMenu()
            );

            return;
        }

        await bot.SendMessage(
            chatId,
            text: $"📅 Select a day:",
            cancellationToken: ct,
            replyMarkup: BotKeyboards.HistoryMenu(days)
        );
    }

    private async Task HandleFoodConfirm(
        ITelegramBotClient bot,
        long chatId,
        int messageId,
        CancellationToken ct
    )
    {
        if (userStateService.GetState(chatId) == UserState.WaitingConfirmation)
        {
            var foodLog = userStateService.GetPendingLog(chatId);

            if ( foodLog != null)
            {
                await databaseService.LogFoodAsync(foodLog);

                await bot.DeleteMessage(
                    chatId,
                    messageId,
                    cancellationToken: ct
                );

                await bot.SendMessage(
                    chatId,
                    text: "*✅ Added!*",
                    cancellationToken: ct,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: BotKeyboards.BackToMainMenu()
                );

                userStateService.SetState(chatId, UserState.Idle);
            }
        }
    }

    private async Task HandleFoodCancel(
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
            text: "Canceled!",
            cancellationToken: ct
        );

        userStateService.SetState(chatId, UserState.Idle);
    }

    private async Task HandleFoodEdit(
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
            text: "*What would you like to edit?*",
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.FoodEditMenu(),
            cancellationToken: ct
        );
    }

    private async Task HandleManageGoal(
        ITelegramBotClient bot,
        long chatId,
        CancellationToken ct
    )
    {
        await bot.SendMessage(
            chatId,
            text: "Enter your body weight (kg):",
            cancellationToken: ct
        );

        userStateService.SetState(chatId, UserState.WaitingGoalWeight);
    }

    private async Task HandleSetGoal(
        ITelegramBotClient bot,
        long chatId,
        CancellationToken ct
    )
    {
        if (userStateService.GetState(chatId) == UserState.WaitingGoalConfirmation)
        {
            var macroGoal = userStateService.GetMacroGoal(chatId);

            if (macroGoal != null)
            {
                await databaseService.LogGoal(chatId, macroGoal);

                await bot.SendMessage(
                    chatId,
                    text: "Macro goal added!",
                    cancellationToken: ct
                );
            }
            else
            {
                await bot.SendMessage(
                    chatId,
                    text: "Error occurred!",
                    cancellationToken: ct
                );
            }

            userStateService.SetState(chatId, UserState.Idle);
        }
    }

    private async Task HandleCancelGoal(
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
            text: "Canceled!",
            cancellationToken: ct
        );

        userStateService.SetState(chatId, UserState.Idle);
    }

    private async Task HandleGenderSelected(
        ITelegramBotClient bot,
        long chatId,
        string data,
        CancellationToken ct
    )
    {
        string gender = data.Replace("goal_gender_", "");
        var setup = userStateService.GetGoalSetup(chatId) ?? new GoalSetup();

        setup.Gender = gender;
        userStateService.SetGoalSetup(chatId, setup);
        await bot.SendMessage(
            chatId,
            text: "Choose your goal:",
            cancellationToken: ct,
            replyMarkup: BotKeyboards.GoalMenu()
        );
    }

    private async Task HandleAimSelected(
        ITelegramBotClient bot,
        long chatId,
        string data,
        CancellationToken ct
    )
    {
        string goal = data.Replace("aim_", "");
        var setup = userStateService.GetGoalSetup(chatId) ?? new GoalSetup();

        setup.Goal = goal;
        userStateService.SetGoalSetup(chatId, setup);

        var macroGoal = TdeeCalculatorService.CalculateMacros(setup);

        await bot.SendMessage(
            chatId,
            text: $"🎯 Your daily goal\n\n🔥 Calories: {macroGoal.Calories} kcal\n🥩 Protein: {macroGoal.Protein}g\n🧈 Fat: {macroGoal.Fat}g\n🍞 Carbs: {macroGoal.Carbs}g",
            cancellationToken: ct,
            replyMarkup: BotKeyboards.GoalConfirmMenu()
        );

        userStateService.SetMacroGoal(chatId, macroGoal);
        userStateService.SetState(chatId, UserState.WaitingGoalConfirmation);
    }

    private async Task HandleHistoryDate(
        ITelegramBotClient bot,
        long chatId,
        string data,
        int messageId,
        CancellationToken ct
    )
    {
        var date = data.Replace("history_", "");
        var dayLogs = await databaseService.GetLogsByDateAsync(chatId, date);

        if (!dayLogs.Any())
        {
            await bot.SendMessage(
                chatId,
                text: "No logs for this day 📭",
                cancellationToken: ct,
                replyMarkup: BotKeyboards.BackToMainMenu()
            );

            return;
        }

        var totalDayKcal = dayLogs.Sum(l => l.Calories).Round();
        var totalDayProtein = dayLogs.Sum(l => l.Protein).Round();
        var totalDayFat = dayLogs.Sum(l => l.Fat).Round();
        var totalDayCarbs = dayLogs.Sum(l => l.Carbs).Round();
        var totalDaySugar = dayLogs.Sum(l => l.Sugar).Round();

        await bot.DeleteMessage(
            chatId,
            messageId,
            cancellationToken: ct
        );

        await bot.SendMessage(
            chatId,
            text: $"📅 {date}\n\n🔥 Calories: {totalDayKcal} kcal\n🥩 Protein: {totalDayProtein}g\n🧈 Fat: {totalDayFat}g\n🍞 Carbs: {totalDayCarbs}g (sugar: {totalDaySugar}g)",
            cancellationToken: ct,
            replyMarkup: BotKeyboards.BackToMainMenu()
        );
    }
}