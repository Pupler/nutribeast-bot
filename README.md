# 🔥 NutriBeast Bot

> **Status: 🚧 In Development**

A Telegram bot for tracking daily calories and macros (protein, fat, carbs), built with C# and .NET Worker Service.

👉 [@NutriBeastBot](https://t.me/NutriBeastBot)

## Features

- 🍗 **Add food** — search by name and grams, auto-fetch macros from Open Food Facts API
- 📊 **Today** — summary of calories and macros for the day
- ✅ **Confirm/Cancel** — review macros before saving
- 💾 **SQLite storage** — all entries saved locally

## Planned

- 📅 History — weekly breakdown
- 🎯 Goals — set daily macro targets
- ✏️ Edit entries

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
