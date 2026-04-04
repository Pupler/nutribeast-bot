using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using NutriBeastBot.Keyboards;
using NutriBeastBot.Constants;
using NutriBeastBot.Services;
using NutriBeastBot.Models;
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

        if (state == UserState.Idle)
        {
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
        }
        else if (state == UserState.WaitingFoodName)
        {
            var parsed = foodParserService.Parse(text);

            if (parsed == null)
            {
                await bot.SendMessage(
                    chatId,
                    text: "Invalid format!\nUse: `chicken breast 200g`",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct
                );

                return;
            }

            var name = parsed.Value.Name;
            var grams = parsed.Value.Grams;
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
            
            var kcal = Math.Round(foodInfo.Calories * grams / 100, 1);
            var protein = Math.Round(foodInfo.Protein * grams / 100, 1);
            var fat = Math.Round(foodInfo.Fat * grams / 100, 1);
            var carbs = Math.Round(foodInfo.Carbs * grams / 100, 1);

            await bot.SendMessage(
                chatId,
                text: $"🍗 {name} ({grams}g)\n\n🔥 Calories: {kcal} kcal\n🥩 Protein: {protein}g\n🧈 Fat: {fat}g\n🍞 Carbs: {carbs}g",
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
                Carbs = carbs
            });

            userStateService.SetState(chatId, UserState.WaitingConfirmation);
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
                await bot.SendMessage(chatId, text: "Write your food name and grams:", cancellationToken: ct);
                userStateService.SetState(chatId, UserState.WaitingFoodName);
                logger.LogInformation("User {ChatId} state: {State}", chatId, userStateService.GetState(chatId));
                break;
            case "check_today": 
                var todayLog = await databaseService.GetTodayLogsAsync(chatId);
                var totalKcal = todayLog.Sum(l => l.Calories);
                var totalProtein = todayLog.Sum(l => l.Protein);
                var totalFat = todayLog.Sum(l => l.Fat);
                var totalCarbs = todayLog.Sum(l => l.Carbs);

                await bot.SendMessage(
                    chatId,
                    text: $"📊 Today's summary\n\n🔥 Calories: {totalKcal} kcal\n🥩 Protein: {totalProtein}g\n🧈 Fat: {totalFat}g\n🍞 Carbs: {totalCarbs}g",
                    cancellationToken: ct
                );
                break;
            case "food_confirm_add":
                var foodLog = userStateService.GetPendingLog(chatId);

                if ( foodLog != null)
                {
                    await databaseService.LogFoodAsync(foodLog);
                }

                await bot.SendMessage(
                    chatId,
                    text: "Added!",
                    cancellationToken: ct
                );

                userStateService.SetState(chatId, UserState.Idle);
                break;
            default:
                break;
        }
    }
}