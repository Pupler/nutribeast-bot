namespace NutriBeastBot.Models;

public class FoodLog
{
    public long ChatId { get; set; }
    public string Name { get; set; } = "";
    public int Grams { get; set; }
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Carbs { get; set; }
}