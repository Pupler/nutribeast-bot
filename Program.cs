using NutriBeastBot;
using NutriBeastBot.Handlers;
using NutriBeastBot.Services;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ITelegramBotClient>(
    new TelegramBotClient(builder.Configuration["BotToken"]!)
);

builder.Services.AddSingleton<UpdateHandler>();

builder.Services.AddSingleton<UserStateService>();

builder.Services.AddSingleton<FoodParserService>();

builder.Services.AddHttpClient<FoodApiService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();