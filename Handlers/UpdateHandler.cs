using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using NutriBeastBot.Keyboards;
using NutriBeastBot.Constants;
using NutriBeastBot.Services;
using NutriBeastBot.Models;
using NutriBeastBot.Extensions;
using Telegram.Bot.Types.Enums;

namespace NutriBeastBot.Handlers;

public partial class UpdateHandler(
    ILogger<UpdateHandler> logger,
    UserStateService userStateService,
    FoodParserService foodParserService,
    FoodApiService foodApiService,
    DatabaseService databaseService
)
{
    public Task HandleErrorAsync(
        ITelegramBotClient bot,
        Exception ex,
        HandleErrorSource source,
        CancellationToken ct
    )
    {
        logger.LogError("Telegram Error: {Error}", ex);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient bot,
        Update update,
        CancellationToken ct
    )
    {
        if (update.Message != null)
        {
            await HandleMessageAsync(bot, update, ct);
        }

        if (update.CallbackQuery != null)
        {
            await HandleCallbackAsync(bot, update, ct);
        }
    }

    public async Task HandleMessageAsync(
        ITelegramBotClient bot,
        Update update,
        CancellationToken ct
    )
    {
        if (update.Message?.Text == null)
        {
            return;
        }

        var text = update.Message.Text;
        var command = text.Split(' ')[0];
        var chatId = update.Message.Chat.Id;
        var state = userStateService.GetState(chatId);

        logger.LogInformation("Message: {Text}", text);

        switch(state)
        {
            case UserState.Idle:
                switch(command)
                {
                    case "/start":
                        await bot.SendMessage(
                            chatId,
                            text: BotTexts.StartMessage,
                            cancellationToken: ct,
                            replyMarkup: BotKeyboards.MainMenu()
                        );
                        break;
                    default:
                        break;
                }
                break;
            case UserState.WaitingFoodName:
                var parsedFood = foodParserService.Parse(text);

                if (parsedFood == null)
                {
                    await bot.SendMessage(
                        chatId,
                        text: "Invalid format!\nUse: `chicken breast 200g`",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: ct
                    );

                    return;
                }

                var name = char.ToUpper(parsedFood.Value.Name[0]) + parsedFood.Value.Name[1..];
                var grams = parsedFood.Value.Grams;

                await bot.SendMessage(
                    chatId,
                    text: $"Searching for {name}...",
                    cancellationToken: ct
                );

                var foodInfo = await foodApiService.SearchFood(name);

                if (foodInfo == null)
                {
                    await bot.SendMessage(
                        chatId,
                        text: "Product not found!",
                        cancellationToken: ct
                    );

                    return;
                }
                
                var kcal = (foodInfo.Calories * grams / 100).Round();
                var protein = (foodInfo.Protein * grams / 100).Round();
                var fat = (foodInfo.Fat * grams / 100).Round();
                var carbs = (foodInfo.Carbs * grams / 100).Round();
                var sugar = (foodInfo.Sugar * grams / 100).Round();

                await bot.SendMessage(
                    chatId,
                    text: $"🍗 {name} ({grams}g)\n\n🔥 Calories: {kcal} kcal\n🥩 Protein: {protein}g\n🧈 Fat: {fat}g\n🍞 Carbs: {carbs}g (sugar: {sugar}g)",
                    cancellationToken: ct,
                    replyMarkup: BotKeyboards.FoodConfirmMenu()
                );

                userStateService.SetPendingLog(chatId, new FoodLog
                {
                    ChatId = chatId,
                    Name = name,
                    Grams = grams,
                    Calories = kcal,
                    Protein = protein,
                    Fat = fat,
                    Carbs = carbs,
                    Sugar = sugar
                });

                userStateService.SetState(chatId, UserState.WaitingConfirmation);
                break;
            case UserState.WaitingGoalWeight:
                if (double.TryParse(text, out var parsedWeight))
                {
                    var setup = userStateService.GetGoalSetup(chatId) ?? new GoalSetup();

                    setup.Weight = parsedWeight;
                    userStateService.SetGoalSetup(chatId, setup);
                    userStateService.SetState(chatId, UserState.WaitingGoalHeight);
                    await bot.SendMessage(
                        chatId,
                        text: "Enter your height (cm):",
                        cancellationToken: ct
                    );
                }
                else
                {
                    await bot.SendMessage(
                        chatId,
                        text: "Invalid weight! Please enter a number (e.g. 70.5):",
                        cancellationToken: ct
                    );
                }

                break;
            case UserState.WaitingGoalHeight:
                if (double.TryParse(text, out var parsedHeight))
                {
                    var setup = userStateService.GetGoalSetup(chatId) ?? new GoalSetup();

                    setup.Height = parsedHeight;
                    userStateService.SetGoalSetup(chatId, setup);
                    userStateService.SetState(chatId, UserState.WaitingGoalAge);
                    await bot.SendMessage(
                        chatId,
                        text: "Enter your age:",
                        cancellationToken: ct
                    );
                }
                else
                {
                    await bot.SendMessage(
                        chatId,
                        text: "Invalid height! Please enter a number (e.g. 180.5):",
                        cancellationToken: ct
                    );
                }

                break;
            case UserState.WaitingGoalAge:
                if (int.TryParse(text, out var parsedAge))
                {
                    var setup = userStateService.GetGoalSetup(chatId) ?? new GoalSetup();

                    setup.Age = parsedAge;
                    userStateService.SetGoalSetup(chatId, setup);
                    userStateService.SetState(chatId, UserState.Idle);
                    await bot.SendMessage(
                        chatId,
                        text: "Choose your gender:",
                        cancellationToken: ct,
                        replyMarkup: BotKeyboards.GenderMenu()
                    );
                }
                break;
            default:
                break;
        }
    }

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