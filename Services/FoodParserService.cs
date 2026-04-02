using System.Text.RegularExpressions;

namespace NutriBeastBot.Services;

public partial class FoodParserService()
{
    [GeneratedRegex(@"\d+g$")]
    private static partial Regex MyRegex();

    public (string Name, int Grams)? Parse(string input)
    {
        var match = MyRegex().Match(input); // (number)g

        if (int.TryParse(match.Value.Replace("g", ""), out var grams))
        {
            var name = input.Replace(match.Value, "").Trim();

            return (name, grams);
        }

        return null;
    }
}