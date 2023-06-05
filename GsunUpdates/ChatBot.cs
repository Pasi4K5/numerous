using System.Text.RegularExpressions;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace GsunUpdates;

public sealed class ChatBot
{
    private readonly OpenAIAPI _api;
    private readonly string _instructions = Config.Get().GptInstructions;
    private IChatEndpoint Chat => _api.Chat;
    private Conversation _conversation;
    private readonly OsuApi _osuApi;

    private static readonly TimeSpan _restartAfter = TimeSpan.FromMinutes(10);
    private DateTime _restartTime = DateTime.Now + _restartAfter;

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

        _conversation.AppendUserInput($"{message.Author.Username}: {await InsertMetadataInto(message.CleanContent)}");

        var response = await _conversation.GetResponseFromChatbotAsync();

        _restartTime = DateTime.Now + _restartAfter;

        return response;
    }

    public void RestartConversation()
    {
        _conversation = Chat.CreateConversation(new ChatRequest
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 1,
            MaxTokens = 1000
        });

        _conversation.AppendSystemMessage(_instructions);

        _restartTime = DateTime.Now + _restartAfter;
    }

    private async Task<string> InsertMetadataInto(string s)
    {
        var urlRegex = new Regex("(?:osu\\.ppy\\.sh\\/beatmapsets\\/|osu\\.ppy\\.sh\\/s\\/)(\\d+)");
        var idRegex = new Regex("\\d+");

        foreach (Match match in urlRegex.Matches(s))
        {
            var url = match.Value;
            var id = idRegex.Match(url).Value;

            var mapData = await _osuApi.RequestAsync("beatmapsets/" + id);
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
