using Discord;
using Discord.WebSocket;
using GsunUpdates;
using OpenAI_API;

var client = new DiscordSocketClient(new()
{
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
});
var db = new JsonDb();
var osuApi = new OsuApi();
var updateService = new UpdateService(client, db, osuApi);
var openAiApi = new OpenAIAPI(Config.Get().OpenAiApiKey);
var chatBot = new ChatBot(openAiApi, osuApi);
var commandHandler = new CommandHandler(client, db, chatBot);
var eventHandler = new Events(client, chatBot, openAiApi);

client.Log += Log;

var token = Config.Get().BotToken;

await client.LoginAsync(TokenType.Bot, token);
await client.StartAsync();

await Task.Delay(-1);

static Task Log(LogMessage msg)
{
    Console.WriteLine(msg.ToString());

    return Task.CompletedTask;
}
