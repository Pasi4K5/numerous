﻿// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Globalization;
using System.Text;
using Discord;
using Numerous.Bot.Util;

namespace Numerous.Bot.Discord.Interactions.Commands;

public partial class AnilistSearchCommandModule
{
    private static Embed[] BuildEmbeds(Media media, Character? character)
    {
        var embeds = new List<Embed> { BuildMediaEmbed(media) };

        var characterEmbed = character is null
            ? null
            : BuildCharacterEmbed(character);

        if (characterEmbed is not null)
        {
            embeds.Add(characterEmbed);
        }

        return embeds.ToArray();
    }

    private static Embed BuildMediaEmbed(Media media)
    {
        var desc = new StringBuilder();

        if (media.Title is { Native: not null, Romaji: not null })
        {
            desc.Append($"**Romaji:** {media.Title?.Romaji}\n");
        }

        if ((media.Title?.Native is not null || media.Title?.Romaji is not null) && media.Title?.English is not null)
        {
            desc.Append($"**English:** {media.Title?.English}\n");
        }

        var builder = new EmbedBuilder()
            .WithColor(
                media.CoverImage?.Color is not null
                    ? new Color(uint.Parse(media.CoverImage.Value.Color[1..], NumberStyles.HexNumber))
                    : _embedDefaultColor
            )
            .WithTitle(media.Title?.Native ?? media.Title?.Romaji ?? media.Title?.English ?? "Unknown")
            .WithDescription($"{desc}\nClick [here]({media.SiteUrl}) to go to Anilist.");

        if (media.CoverImage is not null)
        {
            builder.WithThumbnailUrl(media.CoverImage.Value.Medium);
        }

        if (media.Description is not null)
        {
            builder.AddField("Description", AnilistSearchCommandModule.ReplaceHtml(media.Description).LimitLength(MaxFieldLength));
        }

        if (media.Format is not null)
        {
            builder.AddField("Format", AnilistSearchCommandModule.MakeReadable(media.Format), true);
        }

        var date = media.StartDate.ToString();

        if (!string.IsNullOrEmpty(date))
        {
            builder.AddField("Release Date", date, true);
        }

        if (media.Status is not null)
        {
            builder.AddField("Status", AnilistSearchCommandModule.ToTitleCase(media.Status), true);
        }

        if (media.AverageScore > 0)
        {
            builder.AddField("Average Score", $"{media.AverageScore}%", true);
        }

        if (media.MeanScore > 0)
        {
            builder.AddField("Mean Score", $"{media.MeanScore}%", true);
        }

        if (media.Popularity > 0)
        {
            builder.AddField("Popularity", $"#{media.Popularity}", true);
        }

        if (media.Trending > 0)
        {
            builder.AddField("Trending", $"#{media.Trending}", true);
        }

        if (media.Favourites > 0)
        {
            builder.AddField("Favourites", $"{media.Favourites}", true);
        }

        if (media.Genres?.Length > 0)
        {
            builder.AddField("Genres", string.Join('\n', media.Genres.Select(x => $"• {x}")), true);
        }

        if (media.NonSpoilerTags.Any())
        {
            builder.AddField(
                "Tags",
                string.Join('\n', media.NonSpoilerTags.Select(tag => $"• *{tag.Rank}%* - {tag.Name}".LimitLength(MaxFieldLength))),
                true
            );
        }

        return builder.Build();
    }

    private static Embed BuildCharacterEmbed(Character character)
    {
        var builder = new EmbedBuilder()
            .WithColor(_embedDefaultColor)
            .WithTitle(character.Name?.Full ?? "Unknown")
            .WithDescription($"Click [here]({character.SiteUrl}) to go to Anilist.");

        if (character.Image is not null)
        {
            builder.WithThumbnailUrl(character.Image.Value.Medium);
        }

        if (character.Description is not null)
        {
            builder.AddField("Description", ReplaceHtml(character.Description)
                .LimitLength(MaxFieldLength)
                // Remove spoilers
                .Split("\n~").First()
            );
        }

        if (character.Gender is not null)
        {
            builder.AddField("Gender", character.Gender, true);
        }

        if (character.Age is not null)
        {
            builder.AddField("Age", character.Age, true);
        }

        if (character.Favourites > 0)
        {
            builder.AddField("Favouries", character.Favourites, true);
        }

        return builder.Build();
    }
}
