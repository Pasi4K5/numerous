using Newtonsoft.Json;

namespace Numerous.ApiClients.Osu.Models;

public record struct OsuUser
{
    [JsonProperty("id")]
    public uint Id { get; init; }

    [JsonProperty("is_bot")]
    public bool IsBot { get; init; }

    [JsonProperty("username")]
    public string Username { get; init; }
}
