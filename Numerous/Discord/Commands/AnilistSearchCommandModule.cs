// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using F23.StringSimilarity;
using GraphQL.Client.Http;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Numerous.ApiClients;
using Color = Discord.Color;

namespace Numerous.Discord.Commands;

[UsedImplicitly]
public sealed partial class AnilistSearchCommandModule(AnilistClient anilist) : CommandModule
{
    private const ushort MaxFieldLength = 1024;

    private static readonly Color _embedDefaultColor = new(0, 171, 255);
    private static readonly Levenshtein _levenshtein = new();

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

        try
        {
            var media = await FindMediaAsync(mediaTitle);

            if (media is null)
            {
                await FollowupAsync("Media not found.");

                return;
            }

            var character = embed?.Author is not null
                ? await FindCharacterAsync(ExtractCharName(embed.Author.Value.Name), media.Value)
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

    private async Task<Media?> FindMediaAsync(string mediaTitle)
    {
        var req = new GraphQLHttpRequest
        {
            Query = MediaQuery,
            Variables = new
            {
                mediaTitle,
            },
        };

        var media = (await anilist.Client.SendQueryAsync<JObject>(req)).Data["Media"]?.ToObject<Media>();

        if (media is null)
        {
            return media;
        }

        string?[] titles =
        [
            media.Value.Title.Romaji,
            media.Value.Title.English,
            media.Value.Title.Native,
        ];

        return titles.Any(t => t is not null && RoughlyEqual(t, mediaTitle, 3)) ? media : null;
    }

    private async ValueTask<Character?> FindCharacterAsync(string charName, Media media)
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

        return queryWords.Sum(queryWord =>
            nameWords.Count(nameWord =>
                RoughlyEqual(nameWord, queryWord))
        );
    }

    private static bool RoughlyEqual(string s1, string s2, float threshold = 1f)
    {
        return _levenshtein.Distance(s1.ToLower(), s2.ToLower())
               <= (float)Math.Min(s1.Length, s2.Length) / 3 * threshold;
    }
}
