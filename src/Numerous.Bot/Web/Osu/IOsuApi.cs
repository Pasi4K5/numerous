// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Runtime.Serialization;
using Numerous.Bot.Web.Osu.Models;
using Refit;

namespace Numerous.Bot.Web.Osu;

[Headers("x-api-version: 20240529")]
public interface IOsuApi
{
    internal const string BaseUrl = "https://osu.ppy.sh";

    private const string Api = "/api/v2";

    /// <summary>
    /// https://osu.ppy.sh/docs/index.html#get-user
    /// </summary>
    [Get(Api + "/users/{user}")]
    Task<ApiOsuUserExtended> GetUserAsync(string user, [Query] string? key = null);

    /// <summary>
    /// https://osu.ppy.sh/docs/index.html#get-user-scores
    /// </summary>
    [Get(Api + "/users/{user}/scores/{type}")]
    Task<ApiScore[]> GetUserScoresAsync(
        int user,
        string type,
        [Query] uint limit
    );

    /// <summary>
    /// https://osu.ppy.sh/docs/index.html#get-user-beatmaps
    /// </summary>
    [Get(Api + "/users/{user}/beatmapsets/{type}")]
    Task<ApiBeatmapsetExtended[]> GetUserBeatmapsetsAsync(
        int user,
        string type,
        [Query] string limit = "100",
        [Query] string offset = "0"
    );

    /// <summary>
    /// https://osu.ppy.sh/docs/index.html#get-apiv2beatmapsetsbeatmapset
    /// </summary>
    [Get(Api + "/beatmapsets/{beatmapsetId}")]
    Task<ApiBeatmapsetExtended> GetBeatmapsetAsync(int beatmapsetId);

    /// <returns>
    /// The first 50 beatmapsets matching the specified category.
    /// </returns>
    [Get(Api + "/beatmapsets/search")]
    Task<ApiBeatmapsetSearchResponse> SearchBeatmapsetsAsync(
        [Query] [AliasAs("s")] BeatmapsetCategory category,
        [Query] BeatmapsetSort sort = BeatmapsetSort.UpdatedDesc
    );

    /// <summary>
    /// https://osu.ppy.sh/docs/index.html#get-beatmap
    /// </summary>
    [Get(Api + "/beatmaps/{beatmapId}")]
    Task<ApiBeatmapExtended> GetBeatmapAsync(int beatmapId);

    /// <summary>
    /// https://osu.ppy.sh/docs/index.html#get-beatmaps
    /// </summary>
    [Get(Api + "/beatmaps")]
    Task<ApiBeatmapsResponse> GetBeatmapsAsync(
        [Query(CollectionFormat.Multi)]
        [AliasAs("ids[]")]
        int[] ids
    );

    /// <summary>
    /// https://osu.ppy.sh/docs/index.html#get-topic-listing
    /// </summary>
    [Get(Api + "/forums/topics")]
    Task<ApiForumTopicListing> GetForumTopicsAsync(
        [Query] [AliasAs("forum_id")] string? forumId = null
    );

    /// <summary>
    /// https://osu.ppy.sh/docs/index.html#get-topic-and-posts
    /// </summary>
    [Get(Api + "/forums/topics/{topicId}")]
    Task<ApiForumTopic> GetForumTopicAsync(
        int topicId,
        [Query] int limit = 50,
        [Query] ForumPostSort sort = ForumPostSort.Asc
    );

    public enum BeatmapsetCategory
    {
        [EnumMember(Value = "any")] Any,
        [EnumMember(Value = "ranked")] Ranked,
        [EnumMember(Value = "qualified")] Qualified,
        [EnumMember(Value = "loved")] Loved,
        [EnumMember(Value = "pending")] Pending,
        [EnumMember(Value = "wip")] Wip,
        [EnumMember(Value = "graveyard")] Graveyard,
    }

    public enum BeatmapsetSort
    {
        [EnumMember(Value = "title_desc")] TitleDesc,
        [EnumMember(Value = "title_asc")] TitleAsc,
        [EnumMember(Value = "artist_desc")] ArtistDesc,
        [EnumMember(Value = "artist_asc")] ArtistAsc,
        [EnumMember(Value = "difficulty_desc")] DifficultyDesc,
        [EnumMember(Value = "difficulty_asc")] DifficultyAsc,
        [EnumMember(Value = "updated_desc")] UpdatedDesc,
        [EnumMember(Value = "updated_asc")] UpdatedAsc,
        [EnumMember(Value = "ranked_desc")] RankedDesc,
        [EnumMember(Value = "ranked_asc")] RankedAsc,
        [EnumMember(Value = "rating_desc")] RatingDesc,
        [EnumMember(Value = "rating_asc")] RatingAsc,
        [EnumMember(Value = "plays_desc")] PlaysDesc,
        [EnumMember(Value = "plays_asc")] PlaysAsc,
        [EnumMember(Value = "favourites_desc")] FavouritesDesc,
        [EnumMember(Value = "favourites_asc")] FavouritesAsc,
    }

    public enum ForumPostSort
    {
        [EnumMember(Value = "id_asc")] Asc,
        [EnumMember(Value = "id_desc")] Desc,
    }
}
