using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using NutriBeastBot.Handlers;

namespace NutriBeastBot;

public class Worker(
    ITelegramBotClient botClient,
    UpdateHandler updateHandler
    ) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        botClient.StartReceiving(
            updateHandler: updateHandler.HandleUpdateAsync,
            errorHandler: updateHandler.HandleErrorAsync,
            receiverOptions: new ReceiverOptions
            {
                AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
            },
            cancellationToken: stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
