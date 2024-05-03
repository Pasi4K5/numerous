// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Bot.ApiClients.Osu.Models;

public record OsuUser
{
    [JsonProperty("id")]
    public uint Id { get; init; }

    [JsonProperty("is_bot")]
    public bool IsBot { get; init; }

    [JsonProperty("username")]
    public string Username { get; init; } = null!;

    [JsonProperty("groups")]
    public OsuUserGroup[]? Groups { get; set; } = Array.Empty<OsuUserGroup>();

    [JsonProperty("graveyard_beatmapset_count")]
    public uint GraveyardBeatmapsetCount { get; init; }

    [JsonProperty("guest_beatmapset_count")]
    public uint GuestBeatmapsetCount { get; init; }

    [JsonProperty("loved_beatmapset_count")]
    public uint LovedBeatmapsetCount { get; init; }

    [JsonProperty("pending_beatmapset_count")]
    public uint PendingBeatmapsetCount { get; init; }

    [JsonProperty("ranked_beatmapset_count")]
    public uint RankedBeatmapsetCount { get; init; }

    public OsuUserGroup[] GetGroups()
    {
        return Groups ?? Array.Empty<OsuUserGroup>();
    }
}

public sealed record OsuUserExtended : OsuUser
{
    [JsonProperty("discord")]
    public string DiscordUsername { get; init; } = null!;
}
