using Microsoft.VisualBasic;
using NutriBeastBot.Constants;
using NutriBeastBot.Extensions;
using NutriBeastBot.Keyboards;
using NutriBeastBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NutriBeastBot.Handlers;

public partial class UpdateHandler
{
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
        string command = text.Split(' ')[0];
        var chatId = update.Message.Chat.Id;
        var state = userStateService.GetState(chatId);

        logger.LogInformation("Message: {Text}", text);

        switch(state)
        {
            case UserState.Idle:
                await HandleIdle(bot, chatId, command, ct);
                break;
            case UserState.WaitingFoodName:
                await HandleWaitingFoodName(bot, chatId, text, ct);
                break;
            case UserState.WaitingGoalWeight:
                await HandleWaitingGoalWeight(bot, chatId, text, ct);
                break;
            case UserState.WaitingGoalHeight:
                await HandleWaitingGoalHeight(bot, chatId, text, ct);
                break;
            case UserState.WaitingGoalAge:
                await HandleWaitingGoalAge(bot, chatId, text, ct);
                break;
            default:
                break;
        }
    }

    private static async Task HandleIdle(
        ITelegramBotClient bot,
        long chatId,
        string command,
        CancellationToken ct
    )
    {
        switch(command)
        {
            case "/start":
                await bot.SendMessage(
                    chatId,
                    text: BotTexts.StartMessage,
                    cancellationToken: ct,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: BotKeyboards.MainMenu()
                );
                break;
            default:
                break;
        }
    }

    private async Task HandleWaitingFoodName(
        ITelegramBotClient bot,
        long chatId,
        string text,
        CancellationToken ct
    )
    {
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
    }

    private async Task HandleWaitingGoalWeight(
        ITelegramBotClient bot,
        long chatId,
        string text,
        CancellationToken ct
    )
    {
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
    }

    private async Task HandleWaitingGoalHeight(
        ITelegramBotClient bot,
        long chatId,
        string text,
        CancellationToken ct
    )
    {
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
    }

    private async Task HandleWaitingGoalAge(
        ITelegramBotClient bot,
        long chatId,
        string text,
        CancellationToken ct
    )
    {
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
    }
}