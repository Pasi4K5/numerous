// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Microsoft.Extensions.Hosting;
using Numerous.Bot.Util;
using Numerous.Bot.Web.Osu;
using Numerous.Common.Services;
using Numerous.Common.Util;
using Numerous.Database.Context;
using Numerous.Database.Dtos;

namespace Numerous.Bot.Discord.Services;

public sealed class OsuUserStatsService(IHost host, IUnitOfWorkFactory uowFactory, IOsuApiRepository osuApi) : HostedService
{
    public override async Task StartAsync(CancellationToken ct)
    {
        var uow = uowFactory.Create();
        var osuUsers = await uow.OsuUsers.GetVerifiedIdsAsync(ct);

        foreach (var userId in osuUsers)
        {
            var time = DateTimeUtil.TimeOfDayFromUserId(userId);

            host.Services.UseScheduler(s => s.ScheduleAsync(() =>
                UpdateStatsAsync(userId, ct)
            ).DailyAt(time.Hour, time.Minute).PreventOverlapping(nameof(OsuUserStatsService) + userId));
        }
    }

    private async Task UpdateStatsAsync(int userId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var apiBeatmapsets = osuApi.GetUserUploadedBeatmapsetsAsync(userId);
        var apiUser = await osuApi.GetUserByIdAsync(userId);

        var userStats = new OsuUserStatsDto
        {
            UserId = userId,
            Timestamp = now,
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

            foreach (var beatmap in set.Beatmaps)
            {
                await innerUow.OnlineBeatmaps.EnsureExistsAsync(new()
                {
                    Id = beatmap.Id,
                    OnlineBeatmapsetId = set.Id,
                }, ct);
            }

            await innerUow.OnlineBeatmapsets.EnsureExistsAsync(setDto, ct);

            var setStats = new BeatmapsetStatsDto
            {
                BeatmapsetId = set.Id,
                Timestamp = now,
                Status = set.Ranked,
                PlayCount = set.PlayCount,
                FavouriteCount = set.FavouriteCount,
            };

            await innerUow.BeatmapsetStats.InsertAsync(setStats, ct);

            var beatmapStats = set.Beatmaps.Select(beatmap => new BeatmapStatsDto
            {
                BeatmapId = beatmap.Id,
                Timestamp = now,
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
