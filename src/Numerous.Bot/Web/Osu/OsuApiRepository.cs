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
    Task<ApiOsuUserExtended> GetUserAsync(string query, bool prioritizeUsername = false);
    Task<ApiOsuUserExtended> GetUserByIdAsync(int userId);
    Task<ApiScore[]> GetRecentScoresAsync(int userId);
    IAsyncEnumerable<ApiBeatmapsetExtended[]> GetUserUploadedBeatmapsetsAsync(int userId);
    Task<ApiBeatmapsetExtended[]> GetUserBeatmapsetsAsync(int userId, ApiBeatmapType type);
    Task<ApiBeatmapsetExtended> GetBeatmapsetAsync(int id);
    Task<ApiBeatmapsetExtended[]> SearchRecentlyChangedBeatmapsetsAsync();
    Task<ApiBeatmapExtended> GetBeatmapAsync(int id);
    Task<ApiForumTopic[]> GetForumTopicsAsync(int? forumId = null);
}

public sealed class OsuApiRepository(IOsuApi api) : IOsuApiRepository
{
    public async Task<ApiOsuUserExtended> GetUserAsync(string query, bool prioritizeUsername = false)
    {
        if (prioritizeUsername)
        {
            try
            {
                return await GetUserByUsernameAsync(query);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
            }
        }

        return await api.GetUserAsync(query);
    }

    public async Task<ApiOsuUserExtended> GetUserByIdAsync(int userId)
    {
        return await api.GetUserAsync(userId.ToString(), "id");
    }

    public async Task<ApiScore[]> GetRecentScoresAsync(int userId)
    {
        return await api.GetUserScoresAsync(userId, "recent", 1000);
    }

    public IAsyncEnumerable<ApiBeatmapsetExtended[]> GetUserUploadedBeatmapsetsAsync(int userId)
    {
        ApiBeatmapType[] types =
        [
            ApiBeatmapType.Graveyard,
            ApiBeatmapType.Guest,
            ApiBeatmapType.Loved,
            ApiBeatmapType.Pending,
            ApiBeatmapType.Ranked,
        ];

        return types.ToAsyncEnumerable().SelectAwait(async type => await GetUserBeatmapsetsAsync(userId, type));
    }

    public async Task<ApiBeatmapsetExtended[]> GetUserBeatmapsetsAsync(int userId, ApiBeatmapType type)
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
            var newBeatmapsets = await api.GetUserBeatmapsetsAsync(userId, typeStr, limit.ToString(), offset: offset.ToString());
            beatmapsets.AddRange(newBeatmapsets);

            if (newBeatmapsets.Length < limit)
            {
                break;
            }
        }

        return beatmapsets.ToArray();
    }

    public async Task<ApiBeatmapsetExtended> GetBeatmapsetAsync(int id)
    {
        return await api.GetBeatmapsetAsync(id);
    }

    public async Task<ApiBeatmapsetExtended[]> SearchRecentlyChangedBeatmapsetsAsync()
    {
        var updatedMapsTask =
            api.SearchBeatmapsetsAsync(IOsuApi.BeatmapsetCategory.Any);
        var rankedQualifiedMapsTask =
            api.SearchBeatmapsetsAsync(IOsuApi.BeatmapsetCategory.Any, IOsuApi.BeatmapsetSort.RankedDesc);

        var (updatedMaps, rankedQualifiedMaps) = await (updatedMapsTask, rankedQualifiedMapsTask);

        return updatedMaps.Beatmapsets.Concat(rankedQualifiedMaps.Beatmapsets)
            .OrderByDescending(x => x.RankedDate ?? x.SubmittedDate)
            .ToArray();
    }

    public async Task<ApiBeatmapExtended> GetBeatmapAsync(int id)
    {
        return await api.GetBeatmapAsync(id);
    }

    private async Task<ApiOsuUserExtended> GetUserByUsernameAsync(string username)
    {
        return await api.GetUserAsync(username, "username");
    }

    public async Task<ApiForumTopic[]> GetForumTopicsAsync(int? forumId = null)
    {
        return (await api.GetForumTopicsAsync(forumId?.ToString())).Topics;
    }
}
