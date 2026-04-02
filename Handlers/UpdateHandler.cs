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
    FoodParserService foodParserService
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

            await bot.SendMessage(
                chatId,
                text: $"Name of product: {name}\nGrams: {grams}g",
                cancellationToken: ct
            );

            userStateService.SetState(chatId, UserState.Idle);
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
            default:
                break;
        }
    }
}