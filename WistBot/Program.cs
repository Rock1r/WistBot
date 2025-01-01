// See https://aka.ms/new-console-template for more information

using WistBot;

try
{
    var config = Config.LoadFromJson("config.json");

    config.UseToken(token =>
    {
        var Bot = new Bot(token);
        Bot.StartReceiving();
    });
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
    Console.ReadLine();
