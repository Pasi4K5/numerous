// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Common.Services;

[JsonObject(MemberSerialization.OptOut)]
public record Config
{
    public ulong OwnerDiscordId { get; init; }
    public string Prefix { get; init; } = "\\";
    public ulong DiscordClientId { get; init; }
    public string DiscordClientSecret { get; init; } = "";
    public string BotToken { get; init; } = "";
    public string DbConnectionString { get; init; } = "";
    public uint OsuClientId { get; init; }
    public string OsuClientSecret { get; init; } = "";
    public bool GuildMode { get; init; } = true;
    public ulong[] GuildIds { get; init; } = [];
    public string AttachmentDirectory { get; init; } = "./data/attachments/";
    public string BeatmapDirectory { get; init; } = "./data/beatmaps/";
    public string BaseUrl { get; init; } = "https://localhost:44333";
    public string SauceNaoApiKey { get; set; } = "";
    public EmojiContainer Emojis { get; init; } = new();

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
