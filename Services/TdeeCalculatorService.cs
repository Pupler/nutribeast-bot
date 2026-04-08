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

    public static double CalculateMacros(GoalSetup setup)
    {
        double goalMultiplier;

        switch (setup.Goal)
        {
            case "bulk":
                goalMultiplier = 300;
                return ((GetBMR(setup) * 1.55) + goalMultiplier).Round();
            case "cut":
                goalMultiplier = -400;
                return ((GetBMR(setup) * 1.55) + goalMultiplier).Round();
            default:
                return (GetBMR(setup) * 1.55).Round();
        }
    }
}