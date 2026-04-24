using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using NutriBeastBot.Services;

namespace NutriBeastBot;

public class ReminderService(
    ITelegramBotClient bot,
    DatabaseService db) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var now = DateTime.Now.ToString("HH:mm");
            var users = await db.GetUsersToRemindAsync(now);

            foreach (var chatId in users)
            {
                await bot.SendMessage(
                    chatId,
                    text: "🔔 *Reminder* 🔔\n\nDon't forget to log your meals today! 🍽️",
                    parseMode: ParseMode.Markdown,
                    cancellationToken: ct
                );
            }

            await Task.Delay(TimeSpan.FromMinutes(1), ct);
        }
    }
}