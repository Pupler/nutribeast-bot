using Telegram.Bot.Types.ReplyMarkups;

namespace NutriBeastBot.Keyboards;

public class BotKeyboards()
{
    public static InlineKeyboardMarkup MainMenu()
    {
        var menu_food_btn = InlineKeyboardButton.WithCallbackData("🍽 Food", "menu_food");
        var menu_stats_btn = InlineKeyboardButton.WithCallbackData("📊 Stats", "menu_stats");
        var manage_goal_btn = InlineKeyboardButton.WithCallbackData("🎯 Goal", "goal_menu");
        var menu_settings_btn = InlineKeyboardButton.WithCallbackData("⚙️ Settings", "menu_settings");
        
        return new InlineKeyboardMarkup([
            [ menu_food_btn, menu_stats_btn ],
            [ manage_goal_btn, menu_settings_btn ]
        ]);
    }

    public static InlineKeyboardMarkup FoodMenu()
    {
        var add_food_btn = InlineKeyboardButton.WithCallbackData("🍗 Add food", "add_food");
        var back_btn = InlineKeyboardButton.WithCallbackData("🔙 Back", "main_menu");

        return new InlineKeyboardMarkup([
            [ add_food_btn ],
            [ back_btn ]
        ]);
    }

    public static InlineKeyboardMarkup StatsMenu()
    {
        var today_btn = InlineKeyboardButton.WithCallbackData("☀️ Today", "check_today");
        var history_btn = InlineKeyboardButton.WithCallbackData("📅 History", "check_history");
        var back_btn = InlineKeyboardButton.WithCallbackData("🔙 Back", "main_menu");

        return new InlineKeyboardMarkup([
            [ today_btn, history_btn ],
            [ back_btn ]
        ]);
    }

    public static InlineKeyboardMarkup GoalManageMenu()
    {
        var view_btn = InlineKeyboardButton.WithCallbackData("👁 View goal", "goal_view");
        var set_btn = InlineKeyboardButton.WithCallbackData("✏️ Set / Update", "manage_goal");
        var delete_btn = InlineKeyboardButton.WithCallbackData("🗑 Delete", "goal_delete");
        var back_btn = InlineKeyboardButton.WithCallbackData("🔙 Back", "main_menu");

        return new InlineKeyboardMarkup([
            [ view_btn ],
            [ set_btn, delete_btn ],
            [ back_btn ]
        ]);
    }

    public static InlineKeyboardMarkup SettingsMenu()
    {
        var language_btn = InlineKeyboardButton.WithCallbackData("🌍 Language", "settings_language");
        var reminders_btn = InlineKeyboardButton.WithCallbackData("🔔 Reminders", "settings_reminders");
        var back_btn = InlineKeyboardButton.WithCallbackData("🔙 Back", "main_menu");

        return new InlineKeyboardMarkup([
            [ language_btn, reminders_btn ],
            [ back_btn ]
        ]);
    }

    public static InlineKeyboardMarkup ReminderMenu(bool isEnabled, string reminderTime)
    {
        var predefinedTimes = new[] { "08:00", "12:00", "18:00" };
        
        var morning_btn = reminderTime == "08:00"
            ? InlineKeyboardButton.WithCallbackData("🌅 08:00 [✅]", "reminder_08")
            : InlineKeyboardButton.WithCallbackData("🌅 08:00", "reminder_08");

        var noon_btn = reminderTime == "12:00"
            ? InlineKeyboardButton.WithCallbackData("🍽️ 12:00 [✅]", "reminder_12")
            : InlineKeyboardButton.WithCallbackData("🍽️ 12:00", "reminder_12");

        var evening_btn = reminderTime == "18:00"
            ? InlineKeyboardButton.WithCallbackData("🌙 18:00 [✅]", "reminder_18")
            : InlineKeyboardButton.WithCallbackData("🌙 18:00", "reminder_18");

        var custom_btn = reminderTime != "" && !predefinedTimes.Contains(reminderTime)
            ? InlineKeyboardButton.WithCallbackData($"⏰ {reminderTime} [✅]", "reminder_custom")
            : InlineKeyboardButton.WithCallbackData("✏️ Custom", "reminder_custom");
        var toggle_btn = isEnabled
            ? InlineKeyboardButton.WithCallbackData("🔔 Enabled", "reminder_toggle")
            : InlineKeyboardButton.WithCallbackData("🔕 Disabled", "reminder_toggle");
        var back_btn = InlineKeyboardButton.WithCallbackData("🔙 Back", "menu_settings");

        return new InlineKeyboardMarkup([
            [ morning_btn, noon_btn ],
            [ evening_btn, custom_btn ],
            [ toggle_btn ],
            [ back_btn ]
        ]);
    }

    public static InlineKeyboardMarkup LanguageMenu()
    {
        var german_btn = InlineKeyboardButton.WithCallbackData("🇩🇪 Deutsch", "lang_de");
        var english_btn = InlineKeyboardButton.WithCallbackData("🇬🇧 English", "lang_en");
        var ukrainian_btn = InlineKeyboardButton.WithCallbackData("🇺🇦 Українська", "lang_ua");
        var french_btn = InlineKeyboardButton.WithCallbackData("🇫🇷 Français", "lang_fr");
        var back_btn = InlineKeyboardButton.WithCallbackData("🔙 Back", "menu_settings");

        return new InlineKeyboardMarkup([
            [ german_btn, english_btn ],
            [ ukrainian_btn, french_btn ],
            [ back_btn ]
        ]);
    }

    public static InlineKeyboardMarkup FoodConfirmMenu()
    {
        var add_btn = InlineKeyboardButton.WithCallbackData("✅ Add", "food_confirm_add");
        var cancel_btn = InlineKeyboardButton.WithCallbackData("🗑 Cancel", "cancel");
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
            .Chunk(3)
            .ToList();

        buttons.Add(
        [
            InlineKeyboardButton.WithCallbackData("🔙 Main menu", "main_menu")
        ]);

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

    public static InlineKeyboardMarkup GoalMenu()
    {
        var bulk_btn = InlineKeyboardButton.WithCallbackData("💪 Bulk", "aim_bulk");
        var cut_btn = InlineKeyboardButton.WithCallbackData("🔥 Cut", "aim_cut");
        var maintain_btn = InlineKeyboardButton.WithCallbackData("⚖️ Maintain", "aim_maintain");

        return new InlineKeyboardMarkup([
            [ bulk_btn, cut_btn, maintain_btn ]
        ]);
    }

    public static InlineKeyboardMarkup GoalConfirmMenu()
    {
        var set_goal_btn = InlineKeyboardButton.WithCallbackData("✅ Set goal", "set_goal");
        var cancel_goal_btn = InlineKeyboardButton.WithCallbackData("❌ Cancel", "cancel");

        return new InlineKeyboardMarkup([
            [ set_goal_btn ],
            [ cancel_goal_btn ]
        ]);
    }

    public static InlineKeyboardMarkup BackToMainMenu()
    {
        var main_menu_btn = InlineKeyboardButton.WithCallbackData("🔙 Main menu", "main_menu");

        return new InlineKeyboardMarkup([
            [ main_menu_btn ]
        ]);
    }

    public static InlineKeyboardMarkup FoodEditMenu()
    {
        var edit_kcal_btn = InlineKeyboardButton.WithCallbackData("🔥 Calories", "edit_calories");
        var edit_protein_btn = InlineKeyboardButton.WithCallbackData("🥩 Protein", "edit_protein");
        var edit_fat_btn = InlineKeyboardButton.WithCallbackData("🧈 Fat", "edit_fat");
        var edit_carbs_btn = InlineKeyboardButton.WithCallbackData("🍞 Carbs", "edit_carbs");
        var cancel_btn = InlineKeyboardButton.WithCallbackData("❌ Cancel", "cancel");

        return new InlineKeyboardMarkup([
            [ edit_kcal_btn, edit_protein_btn ],
            [ edit_fat_btn, edit_carbs_btn ],
            [ cancel_btn ]
        ]);
    }

    public static InlineKeyboardMarkup CancelMenu()
    {
        var cancel_btn = InlineKeyboardButton.WithCallbackData("❌ Cancel", "cancel");

        return new InlineKeyboardMarkup([
            [ cancel_btn ]
        ]);
    }
}