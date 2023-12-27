using Newtonsoft.Json;

namespace Numerous.ApiClients.Osu.Models;

public record struct OsuScore
{
    [JsonProperty("id")]
    public ulong Id { get; init; }

    [JsonProperty("rank")]
    public uint Rank { get; init; }

    [JsonProperty("mode_int")]
    public byte Mode { get; init; }
}
