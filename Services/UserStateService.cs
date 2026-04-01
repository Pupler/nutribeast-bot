using NutriBeastBot.Models;

namespace NutriBeastBot.Services;

public class UserStateService
{
    private Dictionary<long, UserState> _states = [];

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
}