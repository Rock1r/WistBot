// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WistBot;
using WistBot.Data;
using WistBot.Data.Repos;
using WistBot.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("config.json");
    builder.Services.AddSingleton<UsersRepo>();
    builder.Services.AddSingleton<WishlistsRepo>();
    builder.Services.AddSingleton<WishlistItemsRepo>();
    builder.Services.AddSingleton<UsersService>();
    builder.Services.AddSingleton<WishListsService>();
    builder.Services.AddSingleton<WishListItemsService>();
    builder.Services.AddDbContext<WistBotDbContext>();
    var app = builder.Build();
    var token = builder.Configuration.GetValue<string>("TelegramBotToken");
    if (string.IsNullOrWhiteSpace(token))
    {
        throw new ArgumentException("Токен не може бути пустим.", nameof(token));
    }
    var Bot = new Bot(token);
    Bot.StartReceiving();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
    Console.ReadLine();
