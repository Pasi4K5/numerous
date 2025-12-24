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
using Serilog;

namespace Numerous.Bot.Discord.Services;

public sealed class OsuUserStatsService
(
    IHost host,
    ILogger logger,
    IUnitOfWorkFactory uowFactory,
    IOsuApiRepository osuApi
)
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

        logger.Information("Starting to track stats for osu! user {UserId} at {Time}", osuUserId, time.TimeOfDay.ToString("hh\\:mm"));

        host.Services.UseScheduler(s => s.ScheduleAsync(() =>
            UpdateStatsAsync(osuUserId, ct)
        ).DailyAt(time.Hour, time.Minute).PreventOverlapping(nameof(OsuUserStatsService) + osuUserId));
    }

    private async Task UpdateStatsAsync(int userId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var userStatsTask = Task.Run(async () =>
        {
            var apiUser = await osuApi.GetUserByIdAsync(userId, ct);

            var userStats = new OsuUserStatsDto
            {
                UserId = userId,
                Timestamp = now,
                FollowerCount = apiUser.FollowerCount,
                SubscriberCount = apiUser.MappingFollowerCount,
                Kudosu = apiUser.Kudosu.Total,
            };

            await using var uow = uowFactory.Create();

            await uow.OsuUserStats.InsertAsync(userStats, ct);
            await uow.CommitAsync(ct);
        }, ct);

        var mapStatsTask = Task.Run(async () =>
        {
            var apiBeatmapsets = await osuApi
                .GetAllUserBeatmapsetsAsync(userId, ct)
                .ToArrayAsync(ct);
            var beatmaps = await osuApi.BulkBeatmapLookupAsync(
                apiBeatmapsets
                    .SelectMany(s => s.Beatmaps)
                    .Select(x => x.Id)
                    .ToArray(),
                ct
            );

            await using var uow = uowFactory.Create();

            // Ensure all users exist

            var userIds = beatmaps
                .SelectMany(x => x.Owners!)
                .Select(x => x.Id)
                .Concat(apiBeatmapsets
                    .Select(x => x.UserId)
                )
                .Distinct()
                .ToArray();

            foreach (var id in userIds)
            {
                await uow.OsuUsers.EnsureExistsAsync(new()
                {
                    Id = id,
                }, ct);
            }

            // Ensure all beatmap(set)s exist
            foreach (var beatmapset in apiBeatmapsets)
            {
                foreach (var beatmap in beatmapset.Beatmaps)
                {
                    await uow.OnlineBeatmaps.EnsureExistsAsync(new()
                    {
                        Id = beatmap.Id,
                        OnlineBeatmapsetId = beatmapset.Id,
                    }, ct);
                }

                await uow.OnlineBeatmapsets.EnsureExistsAsync(new()
                {
                    Id = beatmapset.Id,
                    CreatorId = beatmapset.UserId,
                }, ct);
            }

            foreach (var set in apiBeatmapsets)
            {
                var currentMaps = beatmaps
                    .Where(x => x.BeatmapsetId == set.Id)
                    .ToArray();

                var setStats = new BeatmapsetStatsDto
                {
                    BeatmapsetId = set.Id,
                    Timestamp = now,
                    Status = set.Ranked,
                    PlayCount = set.PlayCount,
                    FavouriteCount = set.FavouriteCount,
                    UserId = userId,
                };

                await uow.BeatmapsetStats.InsertAsync(setStats, ct);

                var beatmapStats = currentMaps.Select(beatmap => new BeatmapStatsDto
                {
                    BeatmapId = beatmap.Id,
                    Timestamp = now,
                    PlayCount = beatmap.PlayCount,
                    PassCount = beatmap.PassCount,
                    UserId = userId,
                    Ownerships = beatmap.Owners!.Select(x => new BeatmapOwnershipStatDto
                    {
                        OwnerId = x.Id,
                        BeatmapId = beatmap.Id,
                        UserId = userId,
                        Timestamp = now,
                    }).ToArray(),
                });

                await uow.BeatmapStats.InsertManyAsync(beatmapStats, ct);
            }

            await uow.CommitAsync(ct);
        }, ct);

        await (userStatsTask, mapStatsTask);
    }
}
