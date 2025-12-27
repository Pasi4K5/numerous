// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Numerous.Common.Util;
using Numerous.DiscordAdapter.Emojis;
using Numerous.DiscordAdapter.Messages.Embeds;

namespace Numerous.DiscordAdapter.DiscordDotNet;

internal static class DiscordMapper
{
    public static Embed ToDiscordEmbed(DiscordEmbed embed) =>
        new EmbedBuilder
        {
            // TODO: Add remaining properties.
            Title = embed.Title,
            Description = embed.Description,
            Color = new(embed.Color.R, embed.Color.G, embed.Color.B),
            Timestamp = embed.Timestamp,
            Url = embed.Url,
            ImageUrl = embed.ImageUrl,
            ThumbnailUrl = embed.ThumbnailUrl,
            Author = embed.Author?.Let(author => new EmbedAuthorBuilder
            {
                Name = author.Name,
                IconUrl = author.IconUrl,
                Url = author.Url,
            }),
        }.Build();

    public static IEmote ToDiscordEmote(DiscordEmoji emoji) =>
        emoji switch
        {
            StandardEmoji e => new Emoji(e.Unicode),
            CustomEmoji e => new Emote(e.Id, ""),
            _ => throw new NotSupportedException($"Unsupported emoji type: {emoji.GetType().FullName}"),
        };
}
