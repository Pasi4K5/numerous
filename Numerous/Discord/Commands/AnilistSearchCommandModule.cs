// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using F23.StringSimilarity;
using GraphQL.Client.Http;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Numerous.ApiClients;

namespace Numerous.Discord.Commands;

[UsedImplicitly]
public sealed partial class AnilistSearchCommandModule(AnilistClient anilist) : CommandModule
{
    private const string MediaQuery =
        """
        query($mediaTitle: String) {
            Media(search: $mediaTitle) {
                id,
                type,
                title {
                    romaji,
                    english,
                    native,
                },
                characters {
                    nodes {
                        id,
                        name {
                            full,
                            alternative,
                            alternativeSpoiler,
                        },
                    },
                },
            },
        }
        """;

    private const string CharQuery =
        """
        query($charName: String) {
            Character(search: $charName) {
                id,
                name {
                    full,
                    alternative,
                    alternativeSpoiler,
                },
            },
        }
        """;

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

            await FollowupAsync(
                msg.GetLink()
                + $"\nhttps://anilist.co/{media.Value.Type.ToLower()}/{media.Value.Id}"
                + (character is null ? "" : $"\nhttps://anilist.co/character/{character.Value.Id}")
            );
        }
        catch (GraphQLHttpRequestException)
        {
            await FollowupAsync("Character not found.");
        }
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

    public void Dispose()
    {
        // _anilistClient.Dispose();
    }

    public record struct Title
    {
        [JsonProperty("native")]
        public required string Native { get; init; }

        [JsonProperty("romaji")]
        public required string Romaji { get; init; }

        [JsonProperty("english")]
        public required string English { get; init; }
    }

    public record struct Media
    {
        [JsonProperty("id")]
        public required int Id { get; init; }

        [JsonProperty("type")]
        public required string Type { get; init; }

        [JsonProperty("title")]
        public required Title Title { get; init; }

        [JsonProperty("characters")]
        public required NodeCollection<Character> Characters { get; init; }
    }

    public record struct NodeCollection<T>
    {
        [JsonProperty("nodes")]
        public required T[] Nodes { get; init; }
    }

    public record struct Character
    {
        [JsonProperty("id")]
        public required int Id { get; init; }

        [JsonProperty("name")]
        public required CharacterName Name { get; init; }
    }

    public record struct CharacterName
    {
        [JsonProperty("full")]
        public required string Full { get; init; }

        [JsonProperty("alternative")]
        public required string[] Alternative { get; init; }

        [JsonProperty("alternativeSpoiler")]
        public required string[] AlternativeSpoiler { get; init; }
    }
}
