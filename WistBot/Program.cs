// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using WistBot;
using WistBot.Data;
using WistBot.Data.Repos;
using WistBot.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("config.json");
    builder.Services.AddScoped<UsersRepo>();
    builder.Services.AddScoped<WishlistsRepo>();
    builder.Services.AddScoped<WishlistItemsRepo>();
    builder.Services.AddScoped<UsersService>();
    builder.Services.AddScoped<WishListsService>();
    builder.Services.AddScoped<WishListItemsService>();
    builder.Services.AddScoped<UserStateManager>();
    builder.Services.AddDbContext<WistBotDbContext>();
    var services = builder.Services.BuildServiceProvider();
    var app = builder.Build();
    var token = builder.Configuration.GetValue<string>("TelegramBotToken");
    if (string.IsNullOrWhiteSpace(token))
    {
        throw new ArgumentException("Токен не може бути пустим.", nameof(token));
    }
    var Bot = new BotService(
        new TelegramBotClient(token),
        (UsersService)services.GetRequiredService(typeof(UsersService)),
        (WishListsService)services.GetRequiredService(typeof(WishListsService)),
        (WishListItemsService)services.GetRequiredService(typeof(WishListItemsService)),
        (UserStateManager)services.GetRequiredService(typeof(UserStateManager))
    );
    Bot.StartReceiving();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
    Console.ReadLine();
