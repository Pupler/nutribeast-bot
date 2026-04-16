using NutriBeastBot.Keyboards;
using NutriBeastBot.Models;
using NutriBeastBot.Services;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace NutriBeastBot.Handlers;

public partial class UpdateHandler
{
    private static async Task HandleGoalMenu(
        ITelegramBotClient bot,
        long chatId,
        int messageId,
        CancellationToken ct
    )
    {
        await bot.DeleteMessage(chatId, messageId, cancellationToken: ct);

        await bot.SendMessage(
            chatId,
            text: "*🎯 Goal settings*",
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.GoalManageMenu(),
            cancellationToken: ct
        );
    }
    
    private async Task HandleManageGoal(
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
            text: "✏️ *Enter your body weight (kg):*",
            cancellationToken: ct,
            parseMode: ParseMode.Markdown
        );

        userStateService.SetState(chatId, UserState.WaitingGoalWeight);
    }

    private async Task HandleSetGoal(
        ITelegramBotClient bot,
        long chatId,
        int messageId,
        CancellationToken ct
    )
    {
        if (userStateService.GetState(chatId) == UserState.WaitingGoalConfirmation)
        {
            var macroGoal = userStateService.GetMacroGoal(chatId);

            if (macroGoal != null)
            {
                await databaseService.LogGoal(chatId, macroGoal);

                await bot.DeleteMessage(
                    chatId,
                    messageId,
                    cancellationToken: ct
                );

                await bot.SendMessage(
                    chatId,
                    text: "🎯 *Macro goal added!*",
                    cancellationToken: ct,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: BotKeyboards.BackToMainMenu()
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
}