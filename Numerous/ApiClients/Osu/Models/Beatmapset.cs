using Newtonsoft.Json;

namespace Numerous.ApiClients.Osu.Models;

[JsonObject(MemberSerialization.OptIn)]
public record struct Beatmapset
{
    private readonly ICollection<string> _tags;

    [JsonProperty("id")]
    public uint Id { get; init; }

    [JsonProperty("artist")]
    public string Artist { get; init; }

    [JsonProperty("title")]
    public string Title { get; init; }

    [JsonProperty("creator")]
    public string Creator { get; init; }

    [JsonProperty("bpm")]
    public double Bpm { get; init; }

    [JsonProperty("tags")]
    public string Tags
    {
        get => string.Join(' ', _tags);
        init => _tags = value.Split(' ');
    }

    [JsonProperty("beatmaps")]
    public IReadOnlyCollection<BeatmapDifficulty> Difficulties { get; init; }
}
