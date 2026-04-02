using System.Text.Json;
using NutriBeastBot.Models;

namespace NutriBeastBot.Services;

public class FoodApiService(HttpClient httpClient)
{
    public async Task<FoodInfo?> SearchFood(string name)
    {
        var url = $"https://world.openfoodfacts.org/cgi/search.pl?search_terms={name.Replace(" ", "+")}&json=true&page_size=1";

        try
        {
            var json = await httpClient.GetStringAsync(url);
            var doc = JsonDocument.Parse(json);
            var product = doc.RootElement.GetProperty("products")[0];
            var nutriments = product.GetProperty("nutriments");

            return new FoodInfo
            {
                Name = product.GetProperty("product_name").GetString() ?? name,
                Calories = nutriments.GetProperty("energy-kcal_100g").GetDouble(),
                Protein = nutriments.GetProperty("proteins_100g").GetDouble(),
                Fat = nutriments.GetProperty("fat_100g").GetDouble(),
                Carbs = nutriments.GetProperty("carbohydrates_100g").GetDouble()
            };
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            return null;
        }
    }
}