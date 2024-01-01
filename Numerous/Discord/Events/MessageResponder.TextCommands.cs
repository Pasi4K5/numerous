using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Numerous.Discord.Events;

public partial class MessageResponder
{
    private const string AnilistEndpoint = "https://graphql.anilist.co";

    private readonly GraphQLHttpClient _anilistClient = new(AnilistEndpoint, new NewtonsoftJsonSerializer());

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

    private async Task<bool> RespondToCommandAsync(SocketMessage msg)
    {
        var content = msg.CleanContent;

        if (!content.StartsWith('%')
            || content.Length != 2
            || msg.Channel is not IGuildChannel)
        {
            return false;
        }

        switch (content[1])
        {
            case 'i':
                if (msg.Reference is null)
                {
                    await msg.ReplyAsync("You must reply to a message to use this command.");

                    return true;
                }

                var embed = (await msg.Channel.GetMessageAsync(msg.Reference.MessageId.Value)).Embeds.FirstOrDefault();
                var mediaTitle = ExtractMediaTitle(embed?.Description);

                if (mediaTitle is null)
                {
                    await msg.ReplyAsync("You must reply to a valid Mudae message to use this command.");

                    return true;
                }

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
                    var media = (await _anilistClient.SendQueryAsync<JObject>(req)).Data["Media"]?.ToObject<Media>();

                    if (media is null)
                    {
                        await msg.ReplyAsync("Media not found.");

                        return true;
                    }

                    var character = embed?.Author is not null
                        ? await FindCharacter(ExtractCharName(embed.Author.Value.Name), media)
                        : null;

                    await msg.ReplyAsync(
                        $"https://anilist.co/{media.Type.ToLower()}/{media.Id}"
                        + (character is null ? "" : $"\nhttps://anilist.co/character/{character.Id}")
                    );
                }
                catch (GraphQLHttpRequestException)
                {
                    await msg.ReplyAsync("Character not found.");
                }

                break;
        }

        return true;
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

        var bestMatch = characters.MaxBy(c => GetCharacterMatchScore(charName, c));

        bestMatch = bestMatch is not null
            ? GetCharacterMatchScore(charName, bestMatch) > 0 ? bestMatch : null
            : null;

        if (bestMatch is not null)
        {
            return bestMatch;
        }

        try
        {
            var response = await _anilistClient.SendQueryAsync<JObject>(new GraphQLHttpRequest
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

        return queryWords.Sum(queryWord =>
            nameWords.Count(nameWord =>
                queryWord.Equals(nameWord, StringComparison.OrdinalIgnoreCase)
            )
        );
    }

    public void Dispose()
    {
        _anilistClient.Dispose();
    }

    public sealed record Title
    {
        [JsonProperty("native")]
        public required string Native { get; init; }

        [JsonProperty("romaji")]
        public required string Romaji { get; init; }

        [JsonProperty("english")]
        public required string English { get; init; }
    }

    public sealed record Media
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

    public sealed record NodeCollection<T>
    {
        [JsonProperty("nodes")]
        public required T[] Nodes { get; init; }
    }

    public sealed record Character
    {
        [JsonProperty("id")]
        public required int Id { get; init; }

        [JsonProperty("name")]
        public required CharacterName Name { get; init; }
    }

    public sealed record CharacterName
    {
        [JsonProperty("full")]
        public required string Full { get; init; }

        [JsonProperty("alternative")]
        public required string[] Alternative { get; init; }

        [JsonProperty("alternativeSpoiler")]
        public required string[] AlternativeSpoiler { get; init; }
    }
}
