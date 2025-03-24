// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Common.Config;

[JsonObject(MemberSerialization.OptOut)]
public record Config
{
    public ulong OwnerDiscordId { get; init; }
    public ulong ExclusiveServerId { get; init; }
    public string Prefix { get; init; } = null!;
    public ulong DiscordClientId { get; init; }
    public string DiscordClientSecret { get; init; } = null!;
    public string BotToken { get; init; } = null!;
    public string DbConnectionString { get; init; } = null!;
    public int OsuClientId { get; init; }
    public string OsuClientSecret { get; init; } = null!;
    public bool GuildMode { get; init; }
    public ulong[] GuildIds { get; init; } = null!;
    public string AttachmentDirectory { get; init; } = null!;
    public string BeatmapDirectory { get; init; } = null!;
    public string BaseUrl { get; init; } = null!;
    public string SauceNaoApiKey { get; set; } = null!;
    public EmojiContainer Emojis { get; init; } = null!;

    public record EmojiContainer
    {
        public ulong RankSilverSs { get; init; }
        public ulong RankSs { get; init; }
        public ulong RankSilverS { get; init; }
        public ulong RankS { get; init; }
        public ulong RankA { get; init; }
        public ulong RankB { get; init; }
        public ulong RankC { get; init; }
        public ulong RankD { get; init; }
        public ulong RankF { get; init; }
    }
}
