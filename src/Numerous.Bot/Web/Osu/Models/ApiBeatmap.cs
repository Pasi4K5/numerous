// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Bot.Web.Osu.Models;

public record ApiBeatmap
{
    [JsonProperty("id")]
    public uint Id { get; set; }

    [JsonProperty("checksum")]
    public string? Checksum { get; set; }

    [JsonProperty("beatmapset")]
    public ApiBeatmapset? Beatmapset { get; init; }

    [JsonProperty("user_id")]
    public uint UserId { get; init; }

    [JsonProperty("mode")]
    public required string Mode { get; init; }
}

public sealed record ApiBeatmapExtended : ApiBeatmap
{
    [JsonProperty("beatmapset")]
    public new ApiBeatmapsetExtended? Beatmapset { get; init; }

    [JsonProperty("max_combo")]
    public uint MaxCombo { get; init; }
}
