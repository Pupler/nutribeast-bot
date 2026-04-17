using Dapper;
using Microsoft.Data.Sqlite;
using NutriBeastBot.Models;

namespace NutriBeastBot.Services;
public class DatabaseService(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("Default")!;
    
    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        
        await connection.ExecuteAsync(@"
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
        ");
    }

    public async Task LogFoodAsync(FoodLog log)
    {
        using var connection = new SqliteConnection(_connectionString);

        await connection.ExecuteAsync(@"
            INSERT INTO food_logs (chat_id, name, grams, calories, protein, fat, carbs, sugar)
            VALUES (@ChatId, @Name, @Grams, @Calories, @Protein, @Fat, @Carbs, @Sugar)
        ", log);
    }

    public async Task<IEnumerable<FoodLog>> GetTodayLogsAsync(long chatId)
    {
        using var connection = new SqliteConnection(_connectionString);

        return await connection.QueryAsync<FoodLog>(@"
            SELECT * FROM food_logs 
            WHERE chat_id = @ChatId 
            AND date(created_at) = date('now')
        ", new { ChatId = chatId });
    }

    public async Task<IEnumerable<string>> GetDaysWithLogsAsync(long chatId)
    {
        using var connection = new SqliteConnection(_connectionString);

        return await connection.QueryAsync<string>(@"
            SELECT DISTINCT date(created_at) as date 
            FROM food_logs 
            WHERE chat_id = @ChatId 
            AND date(created_at) >= date('now', '-7 days')
            ORDER BY date DESC
        ", new { ChatId = chatId });
    }

    public async Task<IEnumerable<FoodLog>> GetLogsByDateAsync(long chatId, string date)
    {
        using var connection = new SqliteConnection(_connectionString);

        return await connection.QueryAsync<FoodLog>(@"
            SELECT * FROM food_logs
            WHERE chat_id = @ChatId
            AND date(created_at) = @Date
        ", new { ChatId = chatId, Date = date });
    }

    public async Task LogGoal(long chatId, MacroGoal macroGoal)
    {
        using var connection = new SqliteConnection(_connectionString);

        await connection.ExecuteAsync(@"
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
        using var connection = new SqliteConnection(_connectionString);

        return await connection.QueryFirstOrDefaultAsync<MacroGoal>(@"
            SELECT * FROM user_goals
            WHERE chat_id = @ChatId
        ", new { ChatId = chatId });
    }

    public async Task DeleteGoal(long chatId)
    {
        using var connection = new SqliteConnection(_connectionString);

        await connection.ExecuteAsync(@"
            DELETE FROM user_goals
            WHERE chat_id = @ChatId
        ", new { ChatId = chatId });
    }
}