using NutriBeastBot.Models;
using NutriBeastBot.Extensions;

namespace NutriBeastBot.Services;

public class TdeeCalculatorService()
{
    public static double GetBMR(GoalSetup setup)
    {
        double bmr = 0;

        switch (setup.Gender)
        {
            case "male":
                bmr = 10 * setup.Weight + 6.25 * setup.Height - 5 * setup.Age + 5;
                break;
            case "female":
                bmr = 10 * setup.Weight + 6.25 * setup.Height - 5 * setup.Age - 161;
                break;
            default:
                break;
        }

        return bmr;
    }

    public static MacroGoal CalculateMacros(GoalSetup setup)
    {
        double goalMultiplier = setup.Goal switch
        {
            "bulk" => 300,
            "cut" => -400,
            _ => 0
        };

        var goalKcal = GetBMR(setup) * 1.55 + goalMultiplier;
        var goalProtein = setup.Weight * 2;
        var goalFat = goalKcal * 0.25 / 9;
        var remainingCalories = goalKcal - goalProtein * 4 - goalFat * 9;

        return new MacroGoal
        {
            Calories = goalKcal.Round(),
            Protein = goalProtein.Round(),
            Fat = goalFat.Round(),
            Carbs = (remainingCalories / 4).Round()
        };
    }
}