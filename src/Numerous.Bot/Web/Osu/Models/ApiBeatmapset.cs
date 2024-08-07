// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Bot.Web.Osu.Models;

[JsonObject(MemberSerialization.OptIn)]
public record ApiBeatmapset
{
    private readonly ICollection<string> _tags = new List<string>();

    [JsonProperty("id")]
    public required uint Id { get; init; }

    [JsonProperty("artist")]
    public required string Artist { get; init; }

    [JsonProperty("title")]
    public required string Title { get; init; }

    [JsonProperty("user_id")]
    public uint UserId { get; set; }

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

    [JsonProperty("covers")]
    public required ApiCovers Covers { get; init; }

    [JsonProperty("play_count")]
    public uint PlayCount { get; init; }

    [JsonProperty("favourite_count")]
    public uint FavouriteCount { get; init; }

    [JsonProperty("beatmaps")]
    public required IReadOnlyCollection<ApiBeatmap> Beatmaps { get; init; }
}

[JsonObject(MemberSerialization.OptIn)]
public record ApiBeatmapsetExtended : ApiBeatmapset
{
    [JsonProperty("submitted_date")]
    public DateTimeOffset? SubmittedDate { get; init; }

    [JsonProperty("ranked_date")]
    public DateTimeOffset? RankedDate { get; init; }

    [JsonProperty("user")]
    public required ApiOsuUser User { get; init; }

    [JsonProperty("related_users")]
    public required IReadOnlyCollection<ApiOsuUser> RelatedUsers { get; init; }
}
