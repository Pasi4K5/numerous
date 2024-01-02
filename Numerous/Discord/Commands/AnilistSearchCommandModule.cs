// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Globalization;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using F23.StringSimilarity;
using GraphQL.Client.Http;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Numerous.ApiClients;
using Numerous.Util;
using Color = Discord.Color;

namespace Numerous.Discord.Commands;

[UsedImplicitly]
public sealed partial class AnilistSearchCommandModule(AnilistClient anilist) : CommandModule
{
    private const string CharFields =
        """
        name {
            full,
            alternative,
            alternativeSpoiler,
        },
        image: image {
            medium,
        },
        description(asHtml: false),
        gender,
        age,
        favourites,
        siteUrl,
        """;

    private const string MediaQuery =
        $$"""
        query($mediaTitle: String) {
            Media(search: $mediaTitle) {
                isAdult,
                title {
                    romaji,
                    english,
                    native,
                },
                description(asHtml: false),
                coverImage {
                    medium,
                    color,
                },
                format,
                status(version: 1),
                startDate {
                    year,
                    month,
                    day,
                },
                averageScore,
                meanScore,
                popularity,
                trending,
                favourites,
                genres,
                tags {
                    name,
                    rank,
                    isGeneralSpoiler,
                    isMediaSpoiler,
                }
                characters {
                    nodes {
                        {{CharFields}}
                    },
                },
                siteUrl,
            },
        }
        """;

    private const string CharQuery =
        $$"""
        query($charName: String) {
            Character(search: $charName) {
                {{CharFields}}
            },
        }
        """;

    private const ushort MaxFieldLength = 1024;

    private static readonly Color _embedDefaultColor = new(0, 171, 255);

    private const string EmojiRegexString = "<:.*?:.*?>";

    [GeneratedRegex(EmojiRegexString)]
    private static partial Regex EmojiRegex();

    [GeneratedRegex("\n" + @$"\*\*\d+\*\*{EmojiRegexString}" + "\n")]
    private static partial Regex KakeraValueRegex();

    [GeneratedRegex(@"\([^)]*\)")]
    private static partial Regex CharacterPostfixRegex();

    [UsedImplicitly]
    [MessageCommand("Search on Anilist")]
    public async Task RespondToCommandAsync(IMessage msg)
    {
        var embed = msg.Embeds.FirstOrDefault();
        var mediaTitle = ExtractMediaTitle(embed?.Description);

        if (mediaTitle is null)
        {
            await RespondAsync("This command can only be used on certain Mudae messages.");

            return;
        }

        await DeferAsync();

        var req = new GraphQLHttpRequest
        {
            Query = MediaQuery,
            Variables = new
            {
                mediaTitle,
            },
        };

        try
        {
            var media = (await anilist.Client.SendQueryAsync<JObject>(req)).Data["Media"]?.ToObject<Media>();

            if (media is null)
            {
                await FollowupAsync("Media not found.");

                return;
            }

            var character = embed?.Author is not null
                ? await FindCharacter(ExtractCharName(embed.Author.Value.Name), media.Value)
                : null;

            if (media.Value.IsAdult)
            {
                await FollowupAsync(
                    msg.GetLink()
                    + "\n**WARNING: This media contains adult content."
                    + "\nIf you are sure that you want to proceed, click the hidden links below.**"
                    + $"\nMedia: ||<{media.Value.SiteUrl}>||"
                    + (character is null ? "" : $"\nCharacter: ||<{character.Value.SiteUrl}>||")
                );

                return;
            }

            await FollowupAsync(
                msg.GetLink(),
                BuildEmbeds(media.Value, character)
            );
        }
        catch (GraphQLHttpRequestException)
        {
            await FollowupAsync("Character not found.");
        }
    }

    private static Embed[] BuildEmbeds(Media media, Character? character)
    {
        var embeds = new List<Embed> { BuildMediaEmbed(media) };

        var characterEmbed = character is null
            ? null
            : BuildCharacterEmbed(character.Value);

        if (characterEmbed is not null)
        {
            embeds.Add(characterEmbed);
        }

        return embeds.ToArray();
    }

    private static Embed BuildMediaEmbed(Media media)
    {
        var builder = new EmbedBuilder()
            .WithColor(
                media.CoverImage?.Color is not null
                    ? new Color(uint.Parse(media.CoverImage.Value.Color[1..], NumberStyles.HexNumber))
                    : _embedDefaultColor
            )
            .WithTitle(media.Title.Romaji)
            .WithDescription($"Click [here]({media.SiteUrl}) to go to Anilist.");

        if (media.CoverImage is not null)
        {
            builder.WithThumbnailUrl(media.CoverImage.Value.Medium);
        }

        if (media.Description is not null)
        {
            builder.AddField("Description", ReplaceHtml(media.Description).LimitLength(MaxFieldLength));
        }

        if (media.Format is not null)
        {
            builder.AddField("Format", MakeReadable(media.Format), true);
        }

        if (media.StartDate is not null)
        {
            builder.AddField("Release Date", media.StartDate.ToString(), true);
        }

        if (media.Status is not null)
        {
            builder.AddField("Status", ToTitleCase(media.Status), true);
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
            .WithTitle(character.Name.Full)
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

    private static string ReplaceHtml(string s)
    {
        return s
            .Replace("<br>", "")
            .Replace("<i>", "*")
            .Replace("</i>", "*")
            .Replace("<b>", "**")
            .Replace("</b>", "**")
            .Replace("<u>", "__")
            .Replace("</u>", "__")
            .Replace("<s>", "~~")
            .Replace("</s>", "~~");
    }

    private static string MakeReadable(string? s)
    {
        return s switch
        {
            "TV_SHORT" => "TV Short",
            "NOVEL" => "Light Novel",
            "OVA" => "OVA",
            "ONA" => "ONA",
            null => "Unknown",
            _ => ToTitleCase(s),
        };
    }

    private static string ToTitleCase(string s)
    {
        return new CultureInfo("en-US").TextInfo.ToTitleCase(s.ToLower().Replace('_', ' '));
    }

    private static string ExtractCharName(string charName)
    {
        return CharacterPostfixRegex().IsMatch(charName)
            ? CharacterPostfixRegex().Replace(charName, "")[..^1]
            : charName;
    }

    private static string? ExtractMediaTitle(string? embedDesc)
    {
        if (embedDesc is null)
        {
            return null;
        }

        if (embedDesc.Contains("React with any emoji to claim!"))
        {
            int lastIndex;

            if (KakeraValueRegex().IsMatch(embedDesc))
            {
                lastIndex = KakeraValueRegex().Match(embedDesc).Index;
            }
            else
            {
                lastIndex = embedDesc.IndexOf("\nReact with any emoji to claim!", StringComparison.Ordinal);

                if (lastIndex < 0)
                {
                    return null;
                }
            }

            embedDesc = embedDesc[..lastIndex];
        }
        else
        {
            if (!EmojiRegex().IsMatch(embedDesc))
            {
                return null;
            }

            var lastIndex = EmojiRegex().Match(embedDesc).Index;

            embedDesc = embedDesc[..lastIndex];
        }

        return embedDesc.Replace('\n', ' ').Trim();
    }

    private async ValueTask<Character?> FindCharacter(string charName, Media media)
    {
        var characters = media.Characters.Nodes;

        Character? bestMatch = characters.Any() ? characters.MaxBy(c => GetCharacterMatchScore(charName, c)) : null;

        bestMatch = bestMatch is not null
            ? GetCharacterMatchScore(charName, bestMatch.Value) > 0 ? bestMatch : null
            : null;

        if (bestMatch is not null)
        {
            return bestMatch;
        }

        try
        {
            var response = await anilist.Client.SendQueryAsync<JObject>(new GraphQLHttpRequest
            {
                Query = CharQuery,
                Variables = new
                {
                    charName,
                },
            });

            return response.Data["Character"]?.ToObject<Character>();
        }
        catch (GraphQLHttpRequestException)
        {
            return null;
        }
    }

    private static int GetCharacterMatchScore(string query, Character character)
    {
        var fullName = character.Name.Full;
        var altNames = character.Name.Alternative
            .Concat(character.Name.AlternativeSpoiler);
        var queryWords = query.Split(' ').Distinct();
        var nameWords = fullName.Split(' ').Concat(altNames).Distinct().ToArray();

        Levenshtein lev = new();

        return queryWords.Sum(queryWord =>
            nameWords.Count(nameWord =>
                lev.Distance(nameWord.ToLower(), queryWord.ToLower()) <= (float)Math.Min(nameWord.Length, queryWord.Length) / 3
            )
        );
    }

    public record struct Media
    {
        [JsonProperty("isAdult")]
        public bool IsAdult { get; init; }

        [JsonProperty("title")]
        public Title Title { get; init; }

        [JsonProperty("description")]
        public string? Description { get; init; }

        [JsonProperty("format")]
        public string? Format { get; init; }

        [JsonProperty("coverImage")]
        public MediaCoverImage? CoverImage { get; init; }

        [JsonProperty("startDate")]
        public FuzzyDate? StartDate { get; init; }

        [JsonProperty("averageScore")]
        public int? AverageScore { get; init; }

        [JsonProperty("meanScore")]
        public int? MeanScore { get; init; }

        [JsonProperty("popularity")]
        public int? Popularity { get; init; }

        [JsonProperty("trending")]
        public int? Trending { get; init; }

        [JsonProperty("favourites")]
        public int? Favourites { get; init; }

        [JsonProperty("genres")]
        public string[]? Genres { get; init; }

        [JsonProperty("tags")]
        public Tag[]? Tags { get; init; }

        [JsonIgnore]
        public readonly IEnumerable<Tag> NonSpoilerTags =>
            Tags?.Where(tag => tag is { IsGeneralSpoiler: false, IsMediaSpoiler: false }) ?? Enumerable.Empty<Tag>();

        [JsonProperty("status")]
        public string? Status { get; init; }

        [JsonProperty("characters")]
        public NodeCollection<Character> Characters { get; init; }

        [JsonProperty("siteUrl")]
        public string? SiteUrl { get; init; }
    }

    public record struct Title
    {
        [JsonProperty("native")]
        public string Native { get; init; }

        [JsonProperty("romaji")]
        public string Romaji { get; init; }

        [JsonProperty("english")]
        public string English { get; init; }
    }

    public readonly record struct FuzzyDate
    {
        [JsonProperty("year")]
        public int? Year { get; init; }

        [JsonProperty("month")]
        public int? Month { get; init; }

        [JsonProperty("day")]
        public int? Day { get; init; }

        public override string ToString()
        {
            return $"{Year}/{Month}/{Day}";
        }
    }

    public readonly record struct Tag
    {
        [JsonProperty("name")]
        public string Name { get; init; }

        [JsonProperty("rank")]
        public int Rank { get; init; }

        [JsonProperty("isGeneralSpoiler")]
        public bool IsGeneralSpoiler { get; init; }

        [JsonProperty("isMediaSpoiler")]
        public bool IsMediaSpoiler { get; init; }
    }

    public record struct NodeCollection<T>
    {
        [JsonProperty("nodes")]
        public T[] Nodes { get; init; }
    }

    public record struct Character
    {
        [JsonProperty("name")]
        public CharacterName Name { get; init; }

        [JsonProperty("image")]
        public MediaCoverImage? Image { get; init; }

        [JsonProperty("description")]
        public string? Description { get; init; }

        [JsonProperty("gender")]
        public string? Gender { get; init; }

        [JsonProperty("age")]
        public string? Age { get; init; }

        [JsonProperty("favourites")]
        public int? Favourites { get; init; }

        [JsonProperty("siteUrl")]
        public string? SiteUrl { get; init; }
    }

    public record struct CharacterName
    {
        [JsonProperty("full")]
        public string Full { get; init; }

        [JsonProperty("alternative")]
        public string[] Alternative { get; init; }

        [JsonProperty("alternativeSpoiler")]
        public string[] AlternativeSpoiler { get; init; }
    }

    public record struct MediaCoverImage
    {
        [JsonProperty("medium")]
        public string? Medium { get; init; }

        [JsonProperty("color")]
        public string? Color { get; init; }
    }
}
