using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using NutriBeastBot.Keyboards;
using NutriBeastBot.Constants;

namespace NutriBeastBot.Handlers;

public class UpdateHandler(
    ILogger<UpdateHandler> logger
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

        logger.LogInformation("Message: {Text}", text);

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

    public Task HandleCallbackAsync(
        ITelegramBotClient bot,
        Update update,
        CancellationToken ct
    )
    {
        if (update.CallbackQuery == null)
        {
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}