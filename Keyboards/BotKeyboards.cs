using Telegram.Bot.Types.ReplyMarkups;

namespace NutriBeastBot.Keyboards;

public class BotKeyboards()
{
    public static InlineKeyboardMarkup MainMenu()
    {
        var add_food_btn = InlineKeyboardButton.WithCallbackData("🍗 Add food", "add_food");
        var today_btn = InlineKeyboardButton.WithCallbackData("📊 Today", "check_today");
        var history_btn = InlineKeyboardButton.WithCallbackData("📅 History", "check_history");
        var goal_btn = InlineKeyboardButton.WithCallbackData("🎯 Goal", "manage_goal");
        
        return new InlineKeyboardMarkup([
            [ add_food_btn ],
            [ today_btn, history_btn ],
            [ goal_btn ]
        ]);
    }
}