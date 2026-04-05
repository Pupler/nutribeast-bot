namespace NutriBeastBot.Extensions;

public static class DoubleExtensions
{
    public static double Round(this double value)
    {
        return Math.Round(value, 1);
    }
}