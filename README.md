# 🔥 NutriBeast Bot

> **Status: 🚧 In Development**

A Telegram bot for tracking daily calories and macros (protein, fat, carbs), built with C# and .NET 8 Worker Service.

👉 [@NutriBeastBot](https://t.me/NutriBeastBot)

## Preview

<img width="300" alt="grafik" src="https://github.com/user-attachments/assets/9e1080d8-4b04-4e93-abbe-8fefc408cc40" />

## Features

- 🍗 **Add food** — search by name and grams, auto-fetch macros from Open Food Facts API
- ✏️ **Edit macros** — adjust calories, protein, fat, carbs before saving
- ✅ **Confirm/Cancel** — review macros before saving to database
- 📊 **Today** — full summary of calories and macros for the day
- 📅 **History** — browse logs by day for the past 7 days
- 🎯 **Goal setup** — enter weight, height, age, gender and goal (bulk/cut/maintain), get personalized daily macro targets calculated via Mifflin-St Jeor formula
- 🎯 **Goal progress** — compare today's intake with daily targets
- 💬 **Clean message design** — structured Telegram UI with emojis and inline actions
- 💾 **SQLite storage** — all entries saved locally with Dapper ORM

## Planned

- 📋 Custom food presets

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
  }
}
```
3. Run:
```bash
dotnet run
```

## License

GPL-3.0
