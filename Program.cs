using NutriBeastBot;
using NutriBeastBot.Handlers;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ITelegramBotClient>(
    new TelegramBotClient(builder.Configuration["BotToken"]!)
);

builder.Services.AddSingleton<UpdateHandler>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();