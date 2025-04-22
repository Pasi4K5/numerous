// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics;
using System.Net;
using Numerous.Bot.Web.Osu.Models;
using Numerous.Common.Util;
using Refit;

namespace Numerous.Bot.Web.Osu;

public interface IOsuApiRepository
{
    Task<ApiOsuUserExtended> GetUserAsync(string query, bool prioritizeUsername = false, CancellationToken ct = default);
    Task<ApiOsuUserExtended> GetUserByIdAsync(int userId, CancellationToken ct = default);
    Task<ApiScore[]> GetRecentScoresAsync(int userId, CancellationToken ct = default);
    IAsyncEnumerable<ApiBeatmapsetExtended[]> GetUserUploadedBeatmapsetsAsync(int userId, CancellationToken ct = default);
    Task<ApiBeatmapsetExtended[]> GetUserBeatmapsetsAsync(int userId, ApiBeatmapType type, CancellationToken ct = default);
    Task<ApiBeatmapsetExtended> GetBeatmapsetAsync(int id, CancellationToken ct = default);
    Task<ApiBeatmapsetExtended[]> SearchRecentlyChangedBeatmapsetsAsync(CancellationToken ct = default);
    Task<ApiBeatmapExtended> GetBeatmapAsync(int id, CancellationToken ct = default);
    Task<ApiBeatmapExtended[]> BulkBeatmapLookupAsync(ICollection<int> beatmapIds, CancellationToken ct = default);
    Task<ApiForumTopicMeta[]> GetForumTopicsAsync(int? forumId = null, CancellationToken ct = default);
    Task<ApiForumTopic> GetForumTopicAsync(int topicId, IOsuApi.ForumPostSort sort, CancellationToken ct = default);
}

public sealed class OsuApiRepository(IOsuApi api) : IOsuApiRepository
{
    public async Task<ApiOsuUserExtended> GetUserAsync(string query, bool prioritizeUsername = false, CancellationToken ct = default)
    {
        if (prioritizeUsername)
        {
            try
            {
                return await GetUserByUsernameAsync(query, ct);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
            }
        }

        return await api.GetUserAsync(query, ct: ct);
    }

    public async Task<ApiOsuUserExtended> GetUserByIdAsync(int userId, CancellationToken ct = default)
    {
        return await api.GetUserAsync(userId.ToString(), "id", ct);
    }

    public async Task<ApiScore[]> GetRecentScoresAsync(int userId, CancellationToken ct = default)
    {
        return await api.GetUserScoresAsync(userId, "recent", 1000, ct);
    }

    public IAsyncEnumerable<ApiBeatmapsetExtended[]> GetUserUploadedBeatmapsetsAsync(int userId, CancellationToken ct = default)
    {
        ApiBeatmapType[] types =
        [
            ApiBeatmapType.Graveyard,
            ApiBeatmapType.Guest,
            ApiBeatmapType.Loved,
            ApiBeatmapType.Pending,
            ApiBeatmapType.Ranked,
        ];

        return types.ToAsyncEnumerable().SelectAwait(async type => await GetUserBeatmapsetsAsync(userId, type, ct));
    }

    public async Task<ApiBeatmapsetExtended[]> GetUserBeatmapsetsAsync(int userId, ApiBeatmapType type, CancellationToken ct = default)
    {
        const int limit = 100;

        var typeStr = type switch
        {
            ApiBeatmapType.Favourite => "favourite",
            ApiBeatmapType.Graveyard => "graveyard",
            ApiBeatmapType.Guest => "guest",
            ApiBeatmapType.Loved => "loved",
            ApiBeatmapType.Nominated => "nominated",
            ApiBeatmapType.Pending => "pending",
            ApiBeatmapType.Ranked => "ranked",
            ApiBeatmapType.MostPlayed => throw new NotSupportedException($"{nameof(ApiBeatmapType.MostPlayed)} is not supported"),
            _ => throw new UnreachableException(),
        };

        var beatmapsets = new List<ApiBeatmapsetExtended>();

        for (var offset = 0;; offset += limit)
        {
            var newBeatmapsets = await api.GetUserBeatmapsetsAsync(userId, typeStr, limit.ToString(), offset: offset.ToString(), ct: ct);
            beatmapsets.AddRange(newBeatmapsets);

            if (newBeatmapsets.Length < limit)
            {
                break;
            }
        }

        return beatmapsets.ToArray();
    }

    public async Task<ApiBeatmapsetExtended> GetBeatmapsetAsync(int id, CancellationToken ct = default)
    {
        return await api.GetBeatmapsetAsync(id, ct);
    }

    public async Task<ApiBeatmapsetExtended[]> SearchRecentlyChangedBeatmapsetsAsync(CancellationToken ct = default)
    {
        var updatedMapsTask =
            api.SearchBeatmapsetsAsync(IOsuApi.BeatmapsetCategory.Any, ct: ct);
        var rankedQualifiedMapsTask =
            api.SearchBeatmapsetsAsync(IOsuApi.BeatmapsetCategory.Any, IOsuApi.BeatmapsetSort.RankedDesc, ct);

        var (updatedMaps, rankedQualifiedMaps) = await (updatedMapsTask, rankedQualifiedMapsTask);

        return updatedMaps.Beatmapsets.Concat(rankedQualifiedMaps.Beatmapsets)
            .OrderByDescending(x => x.RankedDate ?? x.SubmittedDate)
            .ToArray();
    }

    public async Task<ApiBeatmapExtended> GetBeatmapAsync(int id, CancellationToken ct = default)
    {
        return await api.GetBeatmapAsync(id, ct);
    }

    public async Task<ApiBeatmapExtended[]> BulkBeatmapLookupAsync(ICollection<int> beatmapIds, CancellationToken ct = default)
    {
        const int maxBeatmapIds = 50;

        if (!beatmapIds.Any())
        {
            return [];
        }

        return (await beatmapIds
                .Distinct()
                .Chunk(maxBeatmapIds)
                .Select(ids => api.GetBeatmapsAsync(ids, ct))
            )
            .SelectMany(x => x.Beatmaps)
            .ToArray();
    }

    private async Task<ApiOsuUserExtended> GetUserByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await api.GetUserAsync(username, "username", ct);
    }

    public async Task<ApiForumTopicMeta[]> GetForumTopicsAsync(int? forumId = null, CancellationToken ct = default)
    {
        return (await api.GetForumTopicsAsync(forumId?.ToString(), ct)).Topics;
    }

    public async Task<ApiForumTopic> GetForumTopicAsync(int topicId, IOsuApi.ForumPostSort sort, CancellationToken ct = default)
    {
        return await api.GetForumTopicAsync(topicId, sort: sort, ct: ct);
    }
}
