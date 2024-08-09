// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Numerous.Database.Context;
using Numerous.Database.Dtos;
using Numerous.Database.Entities;

namespace Numerous.Database.Repositories;

public interface IBeatmapCompetitionRepository : IRepository<BeatmapCompetitionDto>
{
    Task<bool> HasActiveCompetitionAsync(ulong guildId, CancellationToken ct = default);

    Task<bool> GuildHasCompetitionDuringAsync(
        ulong guildId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken ct = default
    );

    Task<BeatmapCompetitionDto?> FindCurrentWithBeatmapAndCreatorAsync(ulong guildId, CancellationToken ct = default);

    Task<BeatmapCompetitionScoreDto?> FindUserTopScoreWithReplayCompBeatmapAsync(
        ulong guildId,
        ulong discordUserId,
        CancellationToken ct = default
    );

    Task EndCompetitionAsync(ulong guildId);
}

public sealed class BeatmapCompetitionRepository(NumerousDbContext context, IMapper mapper)
    : Repository<DbBeatmapCompetition, BeatmapCompetitionDto>(context, mapper), IBeatmapCompetitionRepository
{
    public async Task<bool> HasActiveCompetitionAsync(ulong guildId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await Set
            .AnyAsync(
                x => x.GuildId == guildId && x.StartTime < now && x.EndTime > now,
                ct
            );
    }

    public async Task<bool> GuildHasCompetitionDuringAsync(
        ulong guildId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken ct = default)
    {
        return await Set
            .AnyAsync(
                x =>
                    x.GuildId == guildId
                    && x.StartTime < endTime.ToUniversalTime()
                    && x.EndTime > startTime.ToUniversalTime(),
                ct
            );
    }

    public async Task<BeatmapCompetitionDto?> FindCurrentWithBeatmapAndCreatorAsync(ulong guildId, CancellationToken ct = default)
    {
        var entity = await GetActiveCompetitionQuery(guildId)
            .Include(x => x.LocalBeatmap.OnlineBeatmap!.Creator)
            .Include(x => x.LocalBeatmap.OnlineBeatmap!.OnlineBeatmapset.Creator)
            .FirstOrDefaultAsync(cancellationToken: ct);

        return Mapper.Map<BeatmapCompetitionDto?>(entity);
    }

    public async Task<BeatmapCompetitionScoreDto?> FindUserTopScoreWithReplayCompBeatmapAsync(
        ulong guildId,
        ulong discordUserId,
        CancellationToken ct = default
    )
    {
        var entity = await GetMostRecentCompetitionQuery(guildId)
            .Include(x => x.Scores)
            .ThenInclude(x => x.Player)
            .SelectMany(x => x.Scores)
            .Include(x => x.Competition.LocalBeatmap)
            .Include(x => x.Replay)
            .Where(x => x.Player.DiscordUserId == discordUserId)
            .OrderByDescending(x => x.TotalScore)
            .FirstOrDefaultAsync(ct);

        return Mapper.Map<BeatmapCompetitionScoreDto?>(entity);
    }

    public async Task EndCompetitionAsync(ulong guildId)
    {
        var now = DateTimeOffset.UtcNow;
        var competition = await GetActiveCompetitionQuery(guildId)
            .FirstOrDefaultAsync();

        if (competition is null)
        {
            return;
        }

        competition.EndTime = now;
    }

    private IQueryable<DbBeatmapCompetition> GetMostRecentCompetitionQuery(ulong guildId)
    {
        return Set.Where(x => x.GuildId == guildId)
            .OrderByDescending(x => x.EndTime)
            .Take(1);
    }

    private IQueryable<DbBeatmapCompetition> GetActiveCompetitionQuery(ulong guildId)
    {
        var now = DateTimeOffset.UtcNow;

        return Set.Where(x => x.GuildId == guildId && x.StartTime < now && x.EndTime > now);
    }
}
