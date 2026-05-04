using Dapper;
using Microsoft.Data.Sqlite;
using NutriBeastBot.Models;

namespace NutriBeastBot.Services;
public class DatabaseService(IConfiguration configuration)
{
    private readonly SqliteConnection _connection = new(configuration.GetConnectionString("Default"));
    
    public async Task InitializeAsync()
    {
        await _connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS food_logs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                chat_id INTEGER NOT NULL,
                name TEXT NOT NULL,
                grams INTEGER NOT NULL,
                calories REAL NOT NULL,
                protein REAL NOT NULL,
                fat REAL NOT NULL,
                carbs REAL NOT NULL,
                sugar REAL NOT NULL,
                created_at TEXT NOT NULL DEFAULT (datetime('now'))
            );
            CREATE TABLE IF NOT EXISTS user_goals (
                chat_id INTEGER PRIMARY KEY,
                calories REAL NOT NULL,
                protein REAL NOT NULL,
                fat REAL NOT NULL,
                carbs REAL NOT NULL
            );
            CREATE TABLE IF NOT EXISTS user_reminders (
                chat_id INTEGER PRIMARY KEY,
                reminder_time TEXT NOT NULL,
                is_enabled INTEGER NOT NULL DEFAULT 1
            );
            CREATE TABLE IF NOT EXISTS user_food_presets (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                chat_id INTEGER NOT NULL,
                name TEXT NOT NULL,
                calories REAL NOT NULL,
                protein REAL NOT NULL,
                fat REAL NOT NULL,
                carbs REAL NOT NULL,
                sugar REAL NOT NULL DEFAULT 0
            );
        ");
    }

    public async Task LogFoodAsync(FoodLog log)
    {
        await _connection.ExecuteAsync(@"
            INSERT INTO food_logs (chat_id, name, grams, calories, protein, fat, carbs, sugar)
            VALUES (@ChatId, @Name, @Grams, @Calories, @Protein, @Fat, @Carbs, @Sugar)
        ", log);
    }

    public async Task<IEnumerable<FoodLog>> GetTodayLogsAsync(long chatId)
    {
        return await _connection.QueryAsync<FoodLog>(@"
            SELECT * FROM food_logs 
            WHERE chat_id = @ChatId 
            AND date(created_at) = date('now')
        ", new { ChatId = chatId });
    }

    public async Task<IEnumerable<string>> GetDaysWithLogsAsync(long chatId)
    {
        return await _connection.QueryAsync<string>(@"
            SELECT DISTINCT date(created_at) as date 
            FROM food_logs 
            WHERE chat_id = @ChatId 
            AND date(created_at) >= date('now', '-7 days')
            ORDER BY date DESC
        ", new { ChatId = chatId });
    }

    public async Task<IEnumerable<FoodLog>> GetLogsByDateAsync(long chatId, string date)
    {
        return await _connection.QueryAsync<FoodLog>(@"
            SELECT * FROM food_logs
            WHERE chat_id = @ChatId
            AND date(created_at) = @Date
        ", new { ChatId = chatId, Date = date });
    }

    public async Task LogGoal(long chatId, MacroGoal macroGoal)
    {
        await _connection.ExecuteAsync(@"
            INSERT OR REPLACE INTO user_goals (chat_id, calories, protein, fat, carbs)
            VALUES (@ChatId, @Calories, @Protein, @Fat, @Carbs)
        ", new
        {
            ChatId = chatId,
            macroGoal.Calories,
            macroGoal.Protein,
            macroGoal.Fat,
            macroGoal.Carbs
        });
    }

    public async Task<MacroGoal?> GetGoal(long chatId)
    {
        return await _connection.QueryFirstOrDefaultAsync<MacroGoal>(@"
            SELECT * FROM user_goals
            WHERE chat_id = @ChatId
        ", new { ChatId = chatId });
    }

    public async Task DeleteGoal(long chatId)
    {
        await _connection.ExecuteAsync(@"
            DELETE FROM user_goals
            WHERE chat_id = @ChatId
        ", new { ChatId = chatId });
    }

    public async Task SetReminderAsync(long chatId, string reminderTime)
    {
        await _connection.ExecuteAsync(@"
            INSERT INTO user_reminders (chat_id, reminder_time, is_enabled)
            VALUES (@chatId, @reminderTime, 1)
            ON CONFLICT(chat_id) DO UPDATE SET reminder_time = @reminderTime, is_enabled = 1
        ", new { chatId, reminderTime });
    }

    public async Task ToggleReminderAsync(long chatId)
    {
        await _connection.ExecuteAsync(@"
            UPDATE user_reminders SET is_enabled = CASE WHEN is_enabled = 1 THEN 0 ELSE 1 END
            WHERE chat_id = @chatId
        ", new { chatId });
    }

    public async Task<IEnumerable<long>> GetUsersToRemindAsync(string currentTime)
    {
        return await _connection.QueryAsync<long>(@"
            SELECT chat_id FROM user_reminders
            WHERE reminder_time = @currentTime AND is_enabled = 1",
            new { currentTime });
    }

    public async Task<bool> IsReminderEnabledAsync(long chatId)
    {
        return await _connection.QueryFirstOrDefaultAsync<bool>(@"
            SELECT is_enabled FROM user_reminders
            WHERE chat_id = @chatId
        ", new { chatId });
    }

    public async Task<string?> GetReminderTime(long chatId)
    {
        return await _connection.QueryFirstOrDefaultAsync<string>(@"
            SELECT reminder_time FROM user_reminders
            WHERE chat_id = @chatId
        ", new { chatId });
    }

    public async Task SaveFoodPresetAsync(long chatId, FoodLog foodLog)
    {
        await _connection.ExecuteAsync(@"
            INSERT INTO user_food_presets (chat_id, name, calories, protein, fat, carbs, sugar)
            VALUES (@chatId, @Name, @Calories, @Protein, @Fat, @Carbs, @Sugar)
        ", new { chatId, foodLog.Name, foodLog.Calories, foodLog.Protein, foodLog.Fat, foodLog.Carbs, foodLog.Sugar });
    }

    public async Task<IEnumerable<FoodLog>> GetFoodPresetsAsync(long chatId)
    {
        return await _connection.QueryAsync<FoodLog>(@"
            SELECT * FROM user_food_presets
            WHERE chat_id = @chatId
        ", new { chatId });
    }

    public async Task DeleteFoodPresetAsync(long chatId, int id)
    {
        await _connection.ExecuteAsync(@"
            DELETE FROM user_food_presets
            WHERE chat_id = @chatId AND id = @id
        ", new { chatId, id });
    }
}