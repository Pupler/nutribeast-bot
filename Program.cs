using NutriBeastBot;
using NutriBeastBot.Handlers;
using NutriBeastBot.Services;
using Polly;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ITelegramBotClient>(
    new TelegramBotClient(builder.Configuration["BotToken"]!)
);

builder.Services.AddSingleton<UpdateHandler>();
builder.Services.AddSingleton<UserStateService>();
builder.Services.AddSingleton<FoodParserService>();
builder.Services.AddSingleton<DatabaseService>();

builder.Services.AddHttpClient<FoodApiService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "NutriBeastBot/1.0");
})
.AddTransientHttpErrorPolicy(policy => 
    policy.WaitAndRetryAsync(10, retryAttempt => 
        TimeSpan.FromSeconds(retryAttempt)));

builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<ReminderService>();

var host = builder.Build();

var db = host.Services.GetRequiredService<DatabaseService>();
await db.InitializeAsync();

host.Run();