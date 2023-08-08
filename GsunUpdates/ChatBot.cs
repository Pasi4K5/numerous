using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace GsunUpdates;

public sealed class ChatBot
{
    public bool IsShutUp => _shutUpUntil > DateTime.Now;

    private readonly OpenAIAPI _api;
    private readonly string _instructions = Config.Get().GptInstructions;
    private IChatEndpoint Chat => _api.Chat;
    private Conversation _conversation;
    private readonly OsuApi _osuApi;

    private static readonly TimeSpan _restartAfter = TimeSpan.FromMinutes(10);
    private DateTime _restartTime = DateTime.Now + _restartAfter;
    private DateTime _shutUpUntil = DateTime.MinValue;
    private readonly Dictionary<ulong, ulong> _lastMessages = new();

    public ChatBot(OpenAIAPI api, OsuApi osuApi)
    {
        _api = api;
        _conversation = Chat.CreateConversation();
        _osuApi = osuApi;

        RestartConversation();
    }

    public async Task<string> GetResponse(SocketMessage message)
    {
        if (_restartTime < DateTime.Now)
        {
            RestartConversation();
        }

        var content = message.CleanContent;

        if (content.StartsWith("@Not Gsun Updates#4025"))
        {
            content = content.Substring(21).TrimStart();
        }

        _conversation.AppendUserInput($"{message.Author.Username}: {await InsertMetadataInto(content)}");

        var response = await _conversation.GetResponseFromChatbotAsync();

        _restartTime = DateTime.Now + _restartAfter;

        return response;
    }

    public void RestartConversation(string? instructions = null, float temperature = 1f)
    {
        _conversation = Chat.CreateConversation(new ChatRequest
        {
            Model = Model.ChatGPTTurbo,
            Temperature = temperature,
            MaxTokens = 1000
        });

        _conversation.AppendSystemMessage(instructions ?? _instructions);

        _restartTime = DateTime.Now + _restartAfter;
    }

    public async Task SendMessageAsync(IMessageChannel channel, string message, IMessage referenceMessage)
    {
        var sentMessage = await channel.SendMessageAsync(message, messageReference: new MessageReference(referenceMessage.Id));

        _lastMessages[channel.Id] = sentMessage.Id;
    }

    public async Task ShutUpAsync(IMessageChannel channel, TimeSpan duration)
    {
        _shutUpUntil = DateTime.Now + duration;

        var lastMessageId = _lastMessages.GetValueOrDefault(channel.Id);

        if (lastMessageId == 0)
        {
            return;
        }

        var message = await channel.GetMessageAsync(lastMessageId);
        await message.DeleteAsync();
    }

    public void Unsilence() => _shutUpUntil = DateTime.MinValue;

    private async Task<string> InsertMetadataInto(string s)
    {
        var urlRegex = new Regex("(?:osu\\.ppy\\.sh\\/beatmapsets\\/|osu\\.ppy\\.sh\\/s\\/)(\\d+)");
        var idRegex = new Regex("\\d+");

        foreach (Match match in urlRegex.Matches(s))
        {
            var url = match.Value;
            var id = idRegex.Match(url).Value;

            var mapData = await _osuApi.RequestAsync("beatmapsets/" + id);

            if (mapData is null)
            {
                continue;
            }

            var data = new JObject
            {
                ["artist"] = mapData["artist"],
                ["title"] = mapData["title"],
                ["creator"] = mapData["creator"],
                ["bpm"] = mapData["bpm"],
                ["status"] = mapData["ranked"],
                ["tags"] = mapData["tags"]
            };

            s = s.Insert(
                s.IndexOf(url, StringComparison.Ordinal) + url.Length,
                $" (Metadata: {data.ToString(Formatting.None)})"
            );
        }

        return s;
    }
}
