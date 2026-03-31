using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using NutriBeastBot.Keyboards;

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
        var command = text.Split(' ')[0];
        var chatId = update.Message.Chat.Id;

        logger.LogInformation("Message: {Text}", text);

        switch(command)
        {
            case "/start":
                var startText = "🔥 NutriBeast — your personal nutrition tracker!\n\nTrack calories, protein, fats & carbs to crush your goals 💪";
                bot.SendMessage(
                    chatId,
                    startText,
                    cancellationToken: ct,
                    replyMarkup: BotKeyboards.MainMenu()
                );
                break;
            default:
                break;
        }

        return Task.CompletedTask;
    }
}