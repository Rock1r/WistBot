// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Resources;
using System.Text;
using Telegram.Bot;
using WistBot;
using WistBot.Core.Actions;
using WistBot.Data;
using WistBot.Data.Repos;
using WistBot.Res;
using WistBot.Services;

try
{
    Console.OutputEncoding = Encoding.UTF8;

    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("config.json");
    builder.Services.AddDbContext<WistBotDbContext>();
    builder.Services.AddScoped<UsersRepo>();
    builder.Services.AddScoped<WishlistsRepo>();
    builder.Services.AddScoped<WishlistItemsRepo>();
    builder.Services.AddScoped<UsersService>();
    builder.Services.AddScoped<WishListsService>();
    builder.Services.AddScoped<WishListItemsService>();
    builder.Services.AddScoped<UserStateManager>();
    builder.Services.AddScoped<LocalizationService>();
    builder.Services.AddSingleton<ResourceManager>(provider =>
    {
        var configuration = provider.GetRequiredService<IConfiguration>();
        var resourceManager = new ResourceManager("WistBot.Res.LocalRes", typeof(LocalRes).Assembly);
        return resourceManager;
    });
    builder.Services.AddSingleton<ITelegramBotClient>(provider =>
    {
        var configuration = provider.GetRequiredService<IConfiguration>();
        var token = configuration.GetValue<string>("TelegramBotToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("TelegramBotToken не може бути пустим.");
        }

        return new TelegramBotClient(token);
    });
    builder.Services.AddSingleton<ActionService>(provider =>
    {
        var actions = provider.GetServices<IBotAction>();
        var localizationService = provider.GetRequiredService<LocalizationService>();
        return ActionService.CreateAsync(actions, localizationService).Result;
    }
    );
    builder.Services.AddScoped<BotService>();

    var actionTypes = typeof(Program).Assembly.GetTypes()
    .Where(type => typeof(IBotAction).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
    foreach (var actionType in actionTypes)
    {
        builder.Services.AddTransient(typeof(IBotAction), actionType);
    }

    var services = builder.Services.BuildServiceProvider();
    var app = builder.Build();

    var bot = services.GetRequiredService<BotService>();
    bot.StartReceiving();
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
    Console.ReadLine();
