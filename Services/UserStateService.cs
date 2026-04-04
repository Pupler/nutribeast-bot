using NutriBeastBot.Models;

namespace NutriBeastBot.Services;

public class UserStateService
{
    private readonly Dictionary<long, UserState> _states = [];
    private readonly Dictionary<long, FoodLog> _pendingLogs = [];

    public UserState GetState(long chatId)
    {
        if (_states.TryGetValue(chatId, out var userState))
        {
            return userState;
        }

        return UserState.Idle;
    }

    public void SetState(long chatId, UserState state)
    {
        _states[chatId] = state;
    }

    public void SetPendingLog(long chatId, FoodLog log)
    {
        _pendingLogs[chatId] = log;
    }

    public FoodLog? GetPendingLog(long chatId)
    {
        if (_pendingLogs.TryGetValue(chatId, out var foodLog))
        {
            return foodLog;
        }

        return null;
    }
}