using NutriBeastBot.Extensions;
using NutriBeastBot.Keyboards;
using NutriBeastBot.Models;
using NutriBeastBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

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

        switch(data)
        {
            case "add_food":
                await bot.SendMessage(
                    chatId,
                    text: "Write your food name and grams:",
                    cancellationToken: ct
                );
                
                userStateService.SetState(chatId, UserState.WaitingFoodName);
                logger.LogInformation("User {ChatId} state: {State}", chatId, userStateService.GetState(chatId));
                break;
            case "check_today": 
                var todayLog = await databaseService.GetTodayLogsAsync(chatId);

                var totalKcal = todayLog.Sum(l => l.Calories).Round();
                var totalProtein = todayLog.Sum(l => l.Protein).Round();
                var totalFat = todayLog.Sum(l => l.Fat).Round();
                var totalCarbs = todayLog.Sum(l => l.Carbs).Round();
                var totalSugar = todayLog.Sum(l => l.Sugar).Round();

                await bot.SendMessage(
                    chatId,
                    text: $"📊 Today's summary\n\n🔥 Calories: {totalKcal} kcal\n🥩 Protein: {totalProtein}g\n🧈 Fat: {totalFat}g\n🍞 Carbs: {totalCarbs}g (sugar: {totalSugar}g)",
                    cancellationToken: ct
                );
                break;
            case "check_history": 
                var days = await databaseService.GetDaysWithLogsAsync(chatId);
                
                if (!days.Any())
                {
                    await bot.SendMessage(
                        chatId,
                        text: "No history yet 📭",
                        cancellationToken: ct
                    );

                    return;
                }

                await bot.SendMessage(
                    chatId,
                    text: $"📅 Select a day:",
                    cancellationToken: ct,
                    replyMarkup: BotKeyboards.HistoryMenu(days)
                );
                break;
            case "food_confirm_add":
                if (userStateService.GetState(chatId) == UserState.WaitingConfirmation)
                {
                    var foodLog = userStateService.GetPendingLog(chatId);

                    if ( foodLog != null)
                    {
                        await databaseService.LogFoodAsync(foodLog);

                        await bot.DeleteMessage(
                            chatId,
                            update.CallbackQuery.Message!.MessageId,
                            cancellationToken: ct
                        );

                        await bot.SendMessage(
                            chatId,
                            text: "Added!",
                            cancellationToken: ct
                        );

                        userStateService.SetState(chatId, UserState.Idle);
                    }
                }

                break;
            case "food_cancel":
                await bot.DeleteMessage(
                    chatId,
                    update.CallbackQuery.Message!.MessageId,
                    cancellationToken: ct
                );

                await bot.SendMessage(
                    chatId,
                    text: "Canceled!",
                    cancellationToken: ct
                );

                userStateService.SetState(chatId, UserState.Idle);
                break;
            case "manage_goal":
                await bot.SendMessage(
                    chatId,
                    text: "Enter your body weight:",
                    cancellationToken: ct
                );

                userStateService.SetState(chatId, UserState.WaitingGoalWeight);
                break;
            case "set_goal":
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
                            text: "Error occured!",
                            cancellationToken: ct
                        );
                    }

                    userStateService.SetState(chatId, UserState.Idle);
                }
                break;
            case "cancel_goal":
                await bot.DeleteMessage(
                    chatId,
                    update.CallbackQuery.Message!.MessageId,
                    cancellationToken: ct
                );

                await bot.SendMessage(
                    chatId,
                    text: "Canceled!",
                    cancellationToken: ct
                );

                userStateService.SetState(chatId, UserState.Idle);
                break;
            default:
                if (data!.StartsWith("goal_gender_"))
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

                if (data!.StartsWith("aim_"))
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

                if (data!.StartsWith("history_"))
                {
                    var date = data.Replace("history_", "");
                    var dayLogs = await databaseService.GetLogsByDateAsync(chatId, date);

                    if (!dayLogs.Any())
                    {
                        await bot.SendMessage(
                            chatId,
                            text: "No logs for this day 📭",
                            cancellationToken: ct
                        );

                        return;
                    }

                    var totalDayKcal = dayLogs.Sum(l => l.Calories).Round();
                    var totalDayProtein = dayLogs.Sum(l => l.Protein).Round();
                    var totalDayFat = dayLogs.Sum(l => l.Fat).Round();
                    var totalDayCarbs = dayLogs.Sum(l => l.Carbs).Round();
                    var totalDaySugar = dayLogs.Sum(l => l.Sugar).Round();

                    await bot.SendMessage(
                        chatId,
                        text: $"📅 {date}\n\n🔥 Calories: {totalDayKcal} kcal\n🥩 Protein: {totalDayProtein}g\n🧈 Fat: {totalDayFat}g\n🍞 Carbs: {totalDayCarbs}g (sugar: {totalDaySugar}g)",
                        cancellationToken: ct
                    );
                }
                break;
        }
    }
}