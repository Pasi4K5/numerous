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
    private readonly OpenAIAPI _api = new(Config.Get().OpenAiApiKey);
    private readonly string _instructions = Config.Get().GptInstructions;
    private IChatEndpoint Chat => _api.Chat;
    private Conversation _conversation;
    private readonly OsuApi _osuApi;

    public ChatBot(OsuApi osuApi)
    {
        _conversation = Chat.CreateConversation();
        _osuApi = osuApi;

        RestartConversation();
    }

    public async Task<string> GetResponse(SocketMessage message)
    {
        _conversation.AppendUserInput($"{message.Author.Username}: {await InsertMetadataInto(message.CleanContent)}");

        return await _conversation.GetResponseFromChatbotAsync();
    }

    public void RestartConversation()
    {
        _conversation = Chat.CreateConversation(new ChatRequest
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.5,
            MaxTokens = 500
        });

        _conversation.AppendSystemMessage(_instructions);
    }

    private async Task<string> InsertMetadataInto(string s)
    {
        var urlRegex = new Regex("(?:osu\\.ppy\\.sh\\/beatmapsets\\/|osu\\.ppy\\.sh\\/s\\/)(\\d+)");
        var idRegex = new Regex("\\d+");

        while (true)
        {
            if (!urlRegex.IsMatch(s))
            {
                break;
            }

            var url = urlRegex.Match(s).Value;
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
