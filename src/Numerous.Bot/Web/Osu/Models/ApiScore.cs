// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Bot.Web.Osu.Models;

public sealed record ApiScore
{
    [JsonProperty("id")]
    public ulong Id { get; init; }

    [JsonProperty("total_score")]
    public uint TotalScore { get; init; }

    [JsonProperty("accuracy")]
    public float Accuracy { get; init; }

    [JsonProperty("max_combo")]
    public uint MaxCombo { get; init; }

    [JsonProperty("statistics")]
    public required ApiStatistics Statistics { get; init; }

    [JsonProperty("ended_at")]
    public DateTimeOffset EndedAt { get; init; }

    [JsonProperty("user")]
    public required ApiOsuUser User { get; init; }

    [JsonProperty("beatmap")]
    public ApiBeatmapExtended? Beatmap { get; init; }

    public sealed record ApiStatistics
    {
        [JsonProperty("great")]
        public uint Great { get; init; }

        [JsonProperty("ok")]
        public uint Ok { get; init; }

        [JsonProperty("meh")]
        public uint Meh { get; init; }

        [JsonProperty("miss")]
        public uint Miss { get; init; }
    }
}
