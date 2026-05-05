# 🔥 NutriBeast Bot
> **Status: ✅ Stable | 🚧 Adding Features**

A Telegram bot for tracking daily calories and macros (protein, fat, carbs), built with C# and .NET 8 Worker Service.

👉 [@NutriBeastBot](https://t.me/NutriBeastBot)

## Preview
<img width="300" alt="grafik" src="https://github.com/user-attachments/assets/9e1080d8-4b04-4e93-abbe-8fefc408cc40" />

## Features
- 🍗 **Add food** — auto search by name and grams via Open Food Facts API
- ✏️ **Edit macros** — adjust calories, protein, fat, carbs before saving
- ✅ **Confirm/Cancel** — review macros before saving to database
- 📊 **Today** — full summary of calories and macros for the day
- 📅 **History** — browse logs by day for the past 7 days
- 🎯 **Goal setup** — personalized macro targets via Mifflin-St Jeor formula (bulk/cut/maintain)
- 📈 **Goal progress** — compare today's intake with daily targets
- 🔔 **Reminders** — set daily notification time with presets or custom input
- 💬 **Clean message design** — structured Telegram UI with emojis and inline actions
- 💾 **SQLite storage** — all entries saved locally with Dapper ORM

## Planned
- 📋 **Custom food presets** — manually add and save foods with custom macros

## Stack
- C# / .NET 8 Worker Service
- Telegram.Bot
- SQLite + Dapper
- Open Food Facts API
- Polly (retry policy)

## Setup
1. Clone the repo
2. Create `appsettings.Development.json`:
```json
{
  "BotToken": "YOUR_TOKEN_HERE",
  "ConnectionStrings": {
    "Default": "Data Source=nutribeast.db"
  },
  "WelcomePhotoId": "YOUR_PHOTO_ID_HERE"
}
```
3. Run:
```bash
dotnet run
```

## License
GPL-3.0
