// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using MongoDB.Bson.Serialization.Attributes;
using Numerous.Bot.ApiClients.Osu;

namespace Numerous.Bot.Database.Entities;

// TODO: Remove comment
// ReSharper disable UnusedMember.Global
[BsonIgnoreExtraElements]
public sealed record GuildOptions : DbEntity<ulong>
{
    public bool TrackMessages { get; init; }
    public bool TrackMemberCount { get; init; }
    public ICollection<OsuRole> OsuRoles { get; init; } = [];
    public ulong? VerificationLogChannel { get; init; } = null;
    public ulong? DeletedMessagesChannel { get; init; } = null;
    public bool AdminsBypassNuMod { get; init; } = true;
    public AutoPingOption[] AutoPingOptions { get; init; } = [];

    public TrackingOptions[] PlayerTrackingOptions { get; init; } = [];

    public IList<ulong> ReadOnlyChannels { get; init; } = Array.Empty<ulong>();
    public DbJoinMessage? JoinMessage { get; set; }
    public ulong? UnverifiedRole { get; set; }

    public record struct TrackingOptions
    {
        public ulong DiscordId { get; init; }
        public bool TrackPlayer { get; init; }
    }

    public record struct OsuRole
    {
        public OsuUserGroup Group { get; init; }
        public ulong RoleId { get; init; }
    }

    public record struct AutoPingOption
    {
        public ulong ChannelId { get; init; }
        public ulong RoleId { get; init; }
        public ulong? Tag { get; init; }
    }

    public sealed record DbJoinMessage(ulong ChannelId, string? Title, string? Description);
}
