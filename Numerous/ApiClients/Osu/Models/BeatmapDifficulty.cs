using Newtonsoft.Json;

namespace Numerous.ApiClients.Osu.Models;

[JsonObject(MemberSerialization.OptIn)]
public record struct BeatmapDifficulty;
