namespace NutriBeastBot.Models;

public enum UserState
{
    Idle,
    WaitingFoodName,
    WaitingConfirmation,
    WaitingGoalWeight,
    WaitingGoalHeight,
    WaitingGoalAge,
    WaitingGoalConfirmation
}