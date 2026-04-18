using NutriBeastBot.Extensions;
using NutriBeastBot.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace NutriBeastBot.Handlers;

public partial class UpdateHandler
{
    private static async Task HandleStatsMenu(
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
            text: "📈 *Stats*\n\nChoose an option 👇",
            parseMode: ParseMode.Markdown,
            cancellationToken: ct,
            replyMarkup: BotKeyboards.StatsMenu()
        );
    }

    private async Task HandleCheckToday(
        ITelegramBotClient bot,
        long chatId,
        int messageId,
        CancellationToken ct
    )
    {
        var todayLog = await databaseService.GetTodayLogsAsync(chatId);

        var totalKcal = todayLog.Sum(l => l.Calories).Round();
        var totalProtein = todayLog.Sum(l => l.Protein).Round();
        var totalFat = todayLog.Sum(l => l.Fat).Round();
        var totalCarbs = todayLog.Sum(l => l.Carbs).Round();
        var totalSugar = todayLog.Sum(l => l.Sugar).Round();

        var getGoal = await databaseService.GetGoal(chatId);

        await bot.DeleteMessage(
            chatId,
            messageId,
            cancellationToken: ct
        );

        await bot.SendMessage(
            chatId,
            text: $"*📊 Today's summary*\n\n🔥 Calories: {totalKcal} / {getGoal?.Calories} kcal\n🥩 Protein: {totalProtein} / {getGoal?.Protein}g\n🧈 Fat: {totalFat} / {getGoal?.Fat}g\n🍞 Carbs: {totalCarbs} (sugar: {totalSugar}g) / {getGoal?.Carbs}g",
            cancellationToken: ct,
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.BackToMainMenu()
        );
    }

    private async Task HandleCheckHistory(
        ITelegramBotClient bot,
        long chatId,
        int messageId,
        CancellationToken ct
    )
    {
        var days = await databaseService.GetDaysWithLogsAsync(chatId);

        await bot.DeleteMessage(
            chatId,
            messageId,
            cancellationToken: ct
        );
        
        if (!days.Any())
        {
            await bot.SendMessage(
                chatId,
                text: "No history yet 📭",
                cancellationToken: ct,
                replyMarkup: BotKeyboards.BackToMainMenu()
            );

            return;
        }

        await bot.SendMessage(
            chatId,
            text: $"*📅 Select a day:*",
            cancellationToken: ct,
            parseMode: ParseMode.Markdown,
            replyMarkup: BotKeyboards.HistoryMenu(days)
        );
    }

    private async Task HandleHistoryDate(
        ITelegramBotClient bot,
        long chatId,
        string data,
        int messageId,
        CancellationToken ct
    )
    {
        var date = data.Replace("history_", "");
        var dayLogs = await databaseService.GetLogsByDateAsync(chatId, date);

        if (!dayLogs.Any())
        {
            await bot.SendMessage(
                chatId,
                text: "No logs for this day 📭",
                cancellationToken: ct,
                replyMarkup: BotKeyboards.BackToMainMenu()
            );

            return;
        }

        var totalDayKcal = dayLogs.Sum(l => l.Calories).Round();
        var totalDayProtein = dayLogs.Sum(l => l.Protein).Round();
        var totalDayFat = dayLogs.Sum(l => l.Fat).Round();
        var totalDayCarbs = dayLogs.Sum(l => l.Carbs).Round();
        var totalDaySugar = dayLogs.Sum(l => l.Sugar).Round();

        await bot.DeleteMessage(
            chatId,
            messageId,
            cancellationToken: ct
        );

        await bot.SendMessage(
            chatId,
            text: $"📅 {date}\n\n🔥 Calories: {totalDayKcal} kcal\n🥩 Protein: {totalDayProtein}g\n🧈 Fat: {totalDayFat}g\n🍞 Carbs: {totalDayCarbs}g (sugar: {totalDaySugar}g)",
            cancellationToken: ct,
            replyMarkup: BotKeyboards.BackToMainMenu()
        );
    }
}