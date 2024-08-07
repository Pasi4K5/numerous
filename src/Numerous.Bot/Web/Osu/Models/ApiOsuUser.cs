// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;
using Numerous.Common.Enums;

namespace Numerous.Bot.Web.Osu.Models;

public record ApiOsuUser
{
    [JsonProperty("id")]
    public uint Id { get; init; }

    [JsonProperty("is_bot")]
    public bool IsBot { get; init; }

    [JsonProperty("username")]
    public required string Username { get; init; }

    [JsonProperty("country_code")]
    public required string CountryCode { get; init; }

    [JsonProperty("groups")]
    public Group[]? Groups { get; set; }

    [JsonProperty("avatar_url")]
    public required string AvatarUrl { get; init; }

    [JsonProperty("cover")]
    public required UserCover Cover { get; init; }

    [JsonProperty("follower_count")]
    public uint FollowerCount { get; init; }

    [JsonProperty("mapping_follower_count")]
    public uint MappingFollowerCount { get; init; }

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

    [JsonProperty("kudosu")]
    public required UserKudosu Kudosu { get; init; }

    public OsuUserGroup[] GetGroups()
    {
        return Groups?.Select(g => (OsuUserGroup)g.Id).ToArray() ?? [];
    }

    public sealed record UserCover
    {
        [JsonProperty("url")]
        public required string Url { get; init; }
    }

    public sealed record UserKudosu
    {
        [JsonProperty("total")]
        public uint Total { get; init; }
    }

    public sealed record Group
    {
        [JsonProperty("id")]
        public uint Id { get; init; }
    }
}

public sealed record ApiOsuUserExtended : ApiOsuUser
{
    [JsonProperty("discord")]
    public required string DiscordUsername { get; init; }
}
