// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Microsoft.Extensions.Hosting;
using Numerous.Bot.Util;
using Numerous.Bot.Web.Osu;
using Numerous.Common.Util;
using Numerous.Database.Context;
using Numerous.Database.Dtos;

namespace Numerous.Bot.Discord.Services;

public sealed class OsuUserStatsService(IHost host, IUnitOfWorkFactory uowFactory, IOsuApiRepository osuApi)
{
    public async Task StartAsync(CancellationToken ct = default)
    {
        var uow = uowFactory.Create();
        var osuUsers = await uow.OsuUsers.GetVerifiedIdsAsync(ct);

        foreach (var userId in osuUsers)
        {
            StartTracking(userId, ct);
        }
    }

    public void StartTracking(int osuUserId, CancellationToken ct = default)
    {
        var time = DateTimeUtil.TimeOfDayFromUserId(osuUserId);

        host.Services.UseScheduler(s => s.ScheduleAsync(() =>
            UpdateStatsAsync(osuUserId, time, ct)
        ).DailyAt(time.Hour, time.Minute).PreventOverlapping(nameof(OsuUserStatsService) + osuUserId));
    }

    private async Task UpdateStatsAsync(int userId, DateTimeOffset time, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var dateTime = now.Date + time.TimeOfDay;

        if (now < dateTime)
        {
            dateTime = dateTime.AddDays(-1);
        }

        var apiBeatmapsets = osuApi.GetUserUploadedBeatmapsetsAsync(userId);
        var apiUser = await osuApi.GetUserByIdAsync(userId);

        var userStats = new OsuUserStatsDto
        {
            UserId = userId,
            Timestamp = dateTime,
            FollowerCount = apiUser.FollowerCount,
            SubscriberCount = apiUser.MappingFollowerCount,
        };

        await using var uow = uowFactory.Create();

        var userStatsTask = uow.OsuUserStats.InsertAsync(userStats, ct);

        var mapStatsTask = apiBeatmapsets.SelectMany(x => x.ToAsyncEnumerable()).ForEachAwaitAsync(async set =>
        {
            var setDto = new OnlineBeatmapsetDto
            {
                Id = set.Id,
                CreatorId = set.UserId,
            };

            await using var innerUow = uowFactory.Create();

            await innerUow.OnlineBeatmapsets.EnsureExistsAsync(setDto, ct);

            foreach (var beatmap in set.Beatmaps)
            {
                await innerUow.OnlineBeatmaps.EnsureExistsAsync(new()
                {
                    Id = beatmap.Id,
                    OnlineBeatmapsetId = set.Id,
                }, ct);
            }

            var setStats = new BeatmapsetStatsDto
            {
                BeatmapsetId = set.Id,
                Timestamp = dateTime,
                Status = set.Ranked,
                PlayCount = set.PlayCount,
                FavouriteCount = set.FavouriteCount,
            };

            await innerUow.BeatmapsetStats.InsertAsync(setStats, ct);

            var beatmapStats = set.Beatmaps.Select(beatmap => new BeatmapStatsDto
            {
                BeatmapId = beatmap.Id,
                Timestamp = dateTime,
                PlayCount = beatmap.PlayCount,
                PassCount = beatmap.PassCount,
            });

            await innerUow.BeatmapStats.InsertManyAsync(beatmapStats, ct);

            await innerUow.CommitAsync(ct);
        }, ct);

        await (userStatsTask, mapStatsTask);
        await uow.CommitAsync(ct);
    }
}
