﻿using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Numerous.ApiClients.Osu;
using Numerous.Configuration;
using Numerous.DependencyInjection;
using OpenAI;
using OpenAI.Chat;
using Message = OpenAI.Chat.Message;

namespace Numerous.ApiClients.OpenAi;

[SingletonService]
public sealed class OpenAiClient
{
    private readonly OpenAIClient _api;
    private readonly string _instructions;
    private ChatEndpoint Chat => _api.ChatEndpoint;
    private readonly List<Message> _conversation = new();
    private readonly OsuApi _osuApi;

    private static readonly TimeSpan _restartAfter = TimeSpan.FromMinutes(10);
    private DateTime _restartTime = DateTime.Now + _restartAfter;

    private const string ChatModel = "gpt-4-1106-preview";

    private readonly IEnumerable<Tool> _tools = new Tool[]
    {
        new Function(
            "osuWikiLookup",
            "Search for articles on the osu! wiki.",
            new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["query"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "The query to search for on the osu! wiki.",
                    },
                },
            }
        ),
        new Function(
            "getOsuWikiArticle",
            "Get the contents of the specified article on the osu! wiki.",
            new JsonObject
            {
                ["type"] = "object",
                ["properties"] = new JsonObject
                {
                    ["path"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["description"] = "The path of the article to get the contents of.",
                    },
                },
            }
        ),
    };

    public OpenAiClient(ConfigManager configManager, OsuApi osuApi)
    {
        _api = new OpenAIClient(configManager.Get().OpenAiApiKey);

        _osuApi = osuApi;

        _instructions = File.ReadAllText(configManager.Get().GptInstructionsPath);

        RestartConversation();
    }

    public async Task<(bool, string)> GetResponseAsync(SocketMessage message)
    {
        if (_restartTime < DateTime.Now)
        {
            RestartConversation();
        }

        _conversation.Add(new(Role.User, $"{message.Author.Username}: {await InsertMetadataInto(message.CleanContent)}"));

        var response = await GetCompletionAsync();

        _conversation.Add(response.FirstChoice.Message);

        var toolCalls = response.FirstChoice.Message.ToolCalls;

        while (toolCalls?.Any() == true)
        {
            if (!await RunToolsAsync(toolCalls[0]))
            {
                return (false, "");
            }

            response = await GetCompletionAsync();

            _conversation.Add(response.FirstChoice.Message);

            toolCalls = response.FirstChoice.Message.ToolCalls;
        }

        _restartTime = DateTime.Now + _restartAfter;

        _conversation.Add(response.FirstChoice.Message);

        return (!string.IsNullOrEmpty(response), response);

        async Task<ChatResponse> GetCompletionAsync()
        {
            return await Chat.GetCompletionAsync(new ChatRequest(
                _conversation,
                _tools,
                "auto",
                ChatModel,
                temperature: 0.3f
            ));
        }
    }

    private async Task<bool> RunToolsAsync(Tool tool)
    {
        var func = tool.Function;

        // TODO: Make it so that new tools don't need to be added in two places.
        switch (func.Name)
        {
            case "osuWikiLookup":
            {
                var args = JsonConvert.DeserializeObject<JObject>(func.Arguments.ToString());
                var query = args?["query"]?.ToString();

                if (query is not null)
                {
                    var result = await _osuApi.WikiLookupAsync(query);

                    if (result is null)
                    {
                        return false;
                    }

                    _conversation.Add(new(tool, result.ToString()));
                }

                break;
            }
            case "getOsuWikiArticle":
            {
                var args = JsonConvert.DeserializeObject<JObject>(func.Arguments.ToString());
                var path = args?["path"]?.ToString();

                if (path is not null)
                {
                    var article = await _osuApi.GetWikiPage(path);

                    if (article is null)
                    {
                        return false;
                    }

                    _conversation.Add(new(tool, article));
                }

                break;
            }
        }

        return true;
    }

    public void RestartConversation(string? instructions = null, float temperature = 1f)
    {
        _conversation.Clear();
        _conversation.Add(new(Role.System, instructions ?? _instructions));

        _restartTime = DateTime.Now + _restartAfter;
    }

    // TODO: Use a beatmap lookup tool instead of inserting metadata manually.
    private async Task<string> InsertMetadataInto(string s)
    {
        var urlRegex = new Regex(@"(?:osu\.ppy\.sh\/beatmapsets\/|osu\.ppy\.sh\/s\/)(\d+)");
        var idRegex = new Regex("\\d+");

        foreach (Match match in urlRegex.Matches(s))
        {
            var url = match.Value;
            var rawId = idRegex.Match(url).Value;

            if (!uint.TryParse(rawId, out var id))
            {
                continue;
            }

            var data = await _osuApi.GetBeatmapsetAsync(id);

            if (data is null)
            {
                continue;
            }

            s = s.Insert(
                s.IndexOf(url, StringComparison.Ordinal) + url.Length,
                $" (Metadata: {data})"
            );
        }

        return s;
    }
}
