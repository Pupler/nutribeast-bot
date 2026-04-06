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

    public static InlineKeyboardMarkup FoodConfirmMenu()
    {
        var add_btn = InlineKeyboardButton.WithCallbackData("✅ Add", "food_confirm_add");
        var cancel_btn = InlineKeyboardButton.WithCallbackData("🗑 Cancel", "food_cancel");
        var edit_btn = InlineKeyboardButton.WithCallbackData("✏️ Edit", "food_edit");

        return new InlineKeyboardMarkup([
            [ add_btn ],
            [ cancel_btn, edit_btn ]
        ]);
    }

    public static InlineKeyboardMarkup HistoryMenu(IEnumerable<string> dates)
    {
        var buttons = dates
            .Select(date => InlineKeyboardButton.WithCallbackData(date, $"history_{date}"))
            .Chunk(3);

        return new InlineKeyboardMarkup(buttons);
    }

    public static InlineKeyboardMarkup GenderMenu()
    {
        var male_btn = InlineKeyboardButton.WithCallbackData("👨 Male", "goal_gender_male");
        var female_btn = InlineKeyboardButton.WithCallbackData("👩 Female", "goal_gender_female");

        return new InlineKeyboardMarkup([
            [ male_btn, female_btn ]
        ]);
    }
}