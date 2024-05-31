// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Bot.ApiClients.Osu.Models;

[JsonObject(MemberSerialization.OptIn)]
public record Beatmapset
{
    private readonly ICollection<string> _tags = new List<string>();

    [JsonProperty("id")]
    public required uint Id { get; init; }

    [JsonProperty("artist")]
    public required string Artist { get; init; }

    [JsonProperty("title")]
    public required string Title { get; init; }

    [JsonProperty("creator")]
    public required string Creator { get; init; }

    [JsonProperty("bpm")]
    public double Bpm { get; init; }

    [JsonProperty("tags")]
    public string Tags
    {
        get => string.Join(' ', _tags);
        init => _tags = value.Split(' ');
    }

    [JsonProperty("play_count")]
    public uint PlayCount { get; init; }

    [JsonProperty("favourite_count")]
    public uint FavouriteCount { get; init; }

    [JsonProperty("beatmaps")]
    public required IReadOnlyCollection<BeatmapDifficulty> Difficulties { get; init; }
}

[JsonObject(MemberSerialization.OptIn)]
public record BeatmapsetExtended : Beatmapset;
