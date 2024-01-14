// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text;
using Newtonsoft.Json;

namespace Numerous.Discord.Commands;

public partial class AnilistSearchCommandModule
{
    public sealed record Media
    {
        [JsonProperty("isAdult")]
        public bool? IsAdult { get; init; }

        [JsonProperty("title")]
        public MediaTitle? Title { get; init; }

        [JsonIgnore]
        public IEnumerable<string> Titles => Title is not null
            ? new[] { Title?.Native, Title?.Romaji, Title?.English }.Where(x => x is not null).Cast<string>()
            : Enumerable.Empty<string>();

        [JsonProperty("synonyms")]
        public string[]? Synonyms { get; init; }

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
        public MediaTag[]? Tags { get; init; }

        [JsonIgnore]
        public IEnumerable<MediaTag> NonSpoilerTags =>
            Tags?.Where(tag => tag is { IsGeneralSpoiler: false, IsMediaSpoiler: false }) ?? Enumerable.Empty<MediaTag>();

        [JsonProperty("status")]
        public string? Status { get; init; }

        [JsonProperty("characters")]
        public NodeCollection<Character>? Characters { get; init; }

        [JsonProperty("siteUrl")]
        public string? SiteUrl { get; init; }
    }

    public readonly record struct MediaTitle
    {
        [JsonProperty("native")]
        public string? Native { get; init; }

        [JsonProperty("romaji")]
        public string? Romaji { get; init; }

        [JsonProperty("english")]
        public string? English { get; init; }
    }

    public readonly record struct FuzzyDate
    {
        [JsonProperty("year")]
        public int? Year { get; init; }

        [JsonProperty("month")]
        public int? Month { get; init; }

        [JsonProperty("day")]
        public int? Day { get; init; }

        public override string? ToString()
        {
            if (Year is null && Month is null && Day is null)
            {
                return null;
            }

            var sb = new StringBuilder();

            if (Year is not null)
            {
                sb.Append(Year);
            }

            if (Month is not null)
            {
                sb.Append('-');
                sb.Append(Month);
            }

            if (Day is not null)
            {
                sb.Append('-');
                sb.Append(Day);
            }

            return sb.ToString();
        }
    }

    public readonly record struct MediaTag
    {
        [JsonProperty("name")]
        public string Name { get; init; }

        [JsonProperty("rank")]
        public int? Rank { get; init; }

        [JsonProperty("isGeneralSpoiler")]
        public bool? IsGeneralSpoiler { get; init; }

        [JsonProperty("isMediaSpoiler")]
        public bool? IsMediaSpoiler { get; init; }
    }

    public sealed class NodeCollection<T>
    {
        [JsonProperty("nodes")]
        public required T[] Nodes { get; init; }
    }

    public sealed record Character
    {
        [JsonProperty("name")]
        public CharacterName? Name { get; init; }

        [JsonProperty("media")]
        public NodeCollection<Media>? ConnectedMedia { get; init; }

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

    public readonly record struct CharacterName
    {
        [JsonProperty("full")]
        public string? Full { get; init; }

        [JsonProperty("alternative")]
        public string[]? Alternative { get; init; }

        [JsonProperty("alternativeSpoiler")]
        public string[]? AlternativeSpoiler { get; init; }

        [JsonIgnore]
        public IEnumerable<string> All => new[]
        {
            Full is not null ? [Full] : Array.Empty<string>(),
            Alternative ?? Array.Empty<string>(),
            AlternativeSpoiler ?? Array.Empty<string>(),
        }.SelectMany(x => x);
    }

    public readonly record struct MediaCoverImage
    {
        [JsonProperty("medium")]
        public string? Medium { get; init; }

        [JsonProperty("color")]
        public string? Color { get; init; }
    }
}
