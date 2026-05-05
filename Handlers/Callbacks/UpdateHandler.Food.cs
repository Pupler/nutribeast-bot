using NutriBeastBot.Extensions;
using NutriBeastBot.Keyboards;
using NutriBeastBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace NutriBeastBot.Handlers;

public partial class UpdateHandler
{
    private static async Task HandleFoodMenu(
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
            text: "🍽 *Food Menu*\n\nChoose an option 👇",
            cancellationToken: ct,
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.FoodMenu()
        );
    }

    private static async Task HandleAddFoodMenu(
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
            text: "🍗 *Add Food*\n\nHow would you like to add food? 👇",
            cancellationToken: ct,
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.AddFoodMenu()
        );
    }
    
    private async Task HandleAddFoodAuto(
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
            replyMarkup: BotKeyboards.CancelMenu()
        );
        
        userStateService.SetState(chatId, UserState.WaitingFoodName);
        // logger.LogInformation("User {ChatId} state: {State}", chatId, userStateService.GetState(chatId));
    }

    private static async Task HandleFoodEdit(
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

    private async Task HandleMacroEdit(
        ITelegramBotClient bot,
        long chatId,
        string data,
        int messageId,
        CancellationToken ct
    )
    {
        var macroEdit = data.Replace("edit_", "");

        await bot.DeleteMessage(
            chatId,
            messageId,
            cancellationToken: ct
        );

        await bot.SendMessage(
            chatId,
            text: $"*Write new {macroEdit} value:*",
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.CancelMenu(),
            cancellationToken: ct
        );

        userStateService.SetState(chatId, UserState.WaitingFoodMacroEdit);
        userStateService.SetMacroEdit(chatId, macroEdit.FirstCharToUpper());
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

            if (foodLog != null)
            {
                await databaseService.LogFoodAsync(foodLog);

                await bot.DeleteMessage(
                    chatId,
                    messageId,
                    cancellationToken: ct
                );

                await bot.SendMessage(
                    chatId,
                    text: "✅ *Logged!*\n\n Every bite counts. Keep it up! 💪",
                    cancellationToken: ct,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: BotKeyboards.BackToMainMenu()
                );

                userStateService.SetState(chatId, UserState.Idle);
            }
        }
    }
}