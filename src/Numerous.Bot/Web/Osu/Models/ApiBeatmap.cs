// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Bot.Web.Osu.Models;

public record ApiBeatmap
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("checksum")]
    public string? Checksum { get; set; }

    [JsonProperty("beatmapset")]
    public ApiBeatmapset? Beatmapset { get; init; }

    [JsonProperty("user_id")]
    public int UserId { get; init; }

    [JsonProperty("mode")]
    public required string Mode { get; init; }

    [JsonProperty("difficulty_rating")]
    public required float DifficultyRating { get; init; }

    [JsonProperty("playcount")]
    public required int PlayCount { get; init; }

    [JsonProperty("passcount")]
    public required int PassCount { get; init; }

    [JsonProperty("owners")]
    public required IReadOnlyCollection<Owner> Owners { get; init; }

    [JsonObject(MemberSerialization.OptIn)]
    public record Owner
    {
        [JsonProperty("id")]
        public required int Id { get; init; }

        [JsonProperty("username")]
        public required string Username { get; init; }
    }
}

public sealed record ApiBeatmapExtended : ApiBeatmap
{
    [JsonProperty("beatmapset")]
    public new ApiBeatmapsetExtended? Beatmapset { get; init; }

    [JsonProperty("max_combo")]
    public int MaxCombo { get; init; }
}
