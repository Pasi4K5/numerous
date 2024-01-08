// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Discord.Commands;

public partial class AnilistSearchCommandModule
{
    public readonly record struct Media
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
