using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

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

    public Task HandleUpdateAsync(
        ITelegramBotClient bot,
        Update update,
        CancellationToken ct
    )
    {
        if (update.Message?.Text == null)
        {
            return Task.CompletedTask;
        }

        var text = update.Message.Text;
        var chatId = update.Message.Chat.Id;

        logger.LogInformation("Message: {Text}", text);
        bot.SendMessage(chatId, text, cancellationToken: ct);

        return Task.CompletedTask;
    }
}