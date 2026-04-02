namespace NutriBeastBot.Services;

public class FoodApiService(HttpClient httpClient)
{
    public async Task<string?> SearchFood(string name)
    {
        var url = $"https://world.openfoodfacts.org/cgi/search.pl?search_terms={name}&json=true&page_size=1";

        try
        {
            var data = await httpClient.GetStringAsync(url);

            return data;
        }
        catch
        {
            return null;
        }
    }
}