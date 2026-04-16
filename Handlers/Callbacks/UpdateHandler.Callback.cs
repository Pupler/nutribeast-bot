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
            case "menu_food":
                await HandleFoodMenu(bot, chatId, messageId, ct);
                break;
            case "menu_stats":
                await HandleStatsMenu(bot, chatId, messageId, ct);
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
                await HandleManageGoal(bot, chatId, ct);
                break;
            case "set_goal":
                await HandleSetGoal(bot, chatId, ct);
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