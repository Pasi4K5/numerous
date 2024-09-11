// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Numerous.Database.Context;
using Numerous.Database.Dtos;
using Numerous.Database.Entities;
using osu.Game.Scoring;

namespace Numerous.Database.Repositories;

public interface IBeatmapCompetitionScoreRepository : IRepository<BeatmapCompetitionScoreDto>
{
    Task InsertAsync(BeatmapCompetitionDto competition, uint osuUserId, BeatmapCompetitionScoreDto score);
    Task<BeatmapCompetitionScoreDto> GetWithPlayerAndBeatmapAsync(Guid hash, CancellationToken ct = default);
    Task<List<BeatmapCompetitionScoreDto>> GetCurrentLeaderboardAsync(ulong guildId, int limit, int offset, CancellationToken ct = default);
    Task<int> GetNumTopScoresAsync(ulong guildId, CancellationToken ct = default);
    Task<int> GetRankOfAsync(BeatmapCompetitionScoreDto dto, ulong guildId, CancellationToken ct = default);
    Task<ulong?> FindTopPlayerDiscordIdAsync(ulong guildId, CancellationToken ct = default);
}

public sealed class BeatmapCompetitionScoreRepository(NumerousDbContext context, IMapper mapper)
    : Repository<DbBeatmapCompetitionScore, BeatmapCompetitionScoreDto>(context, mapper), IBeatmapCompetitionScoreRepository
{
    public override Task InsertAsync(BeatmapCompetitionScoreDto score, CancellationToken ct = default)
    {
        throw new NotSupportedException($"Use {nameof(InsertAsync)}({nameof(BeatmapCompetitionDto)}, {nameof(ScoreInfo)}) instead.");
    }

    public async Task InsertAsync(BeatmapCompetitionDto competition, uint osuUserId, BeatmapCompetitionScoreDto score)
    {
        var entity = Mapper.Map<DbBeatmapCompetitionScore>(score);

        entity.GuildId = competition.GuildId;
        entity.StartTime = competition.StartTime;
        entity.PlayerId = osuUserId;

        await Set.AddAsync(entity);
    }

    public async Task<BeatmapCompetitionScoreDto> GetWithPlayerAndBeatmapAsync(Guid hash, CancellationToken ct = default)
    {
        var entity = await Set
            .Include(x => x.Player)
            .Include(x => x.Competition.LocalBeatmap)
            .FirstOrDefaultAsync(x => x.Id == hash, ct);

        return Mapper.Map<BeatmapCompetitionScoreDto>(entity);
    }

    public async Task<List<BeatmapCompetitionScoreDto>> GetCurrentLeaderboardAsync(ulong guildId, int limit, int offset, CancellationToken ct = default)
    {
        var entities = await Set
            .Include(x => x.Player)
            .Include(x => x.Competition.LocalBeatmap)
            .Where(IsTopScore(guildId))
            .OrderByDescending(x => x.TotalScore)
            .ThenBy(x => x.DateTime)
            .Skip(offset)
            .Take(limit)
            .ToListAsync(ct);

        return Mapper.Map<List<BeatmapCompetitionScoreDto>>(entities);
    }

    public async Task<int> GetNumTopScoresAsync(ulong guildId, CancellationToken ct = default)
    {
        return await Set
            .Where(x =>
                x.GuildId == guildId
                && Context
                    .BeatmapCompetitions
                    .Where(y => y.GuildId == guildId)
                    .OrderByDescending(y => y.EndTime)
                    .Take(1)
                    .Select(y => y.StartTime)
                    .Contains(x.StartTime)
            )
            .Select(x => x.PlayerId)
            .Distinct()
            .CountAsync(ct);
    }

    public async Task<int> GetRankOfAsync(BeatmapCompetitionScoreDto dto, ulong guildId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await Set
                   .Where(IsTopScore(guildId))
                   .CountAsync(x =>
                       x.Competition.StartTime <= now
                       && x.Competition.EndTime >= now
                       && (
                           x.TotalScore > dto.TotalScore
                           || x.TotalScore == dto.TotalScore && x.DateTime < dto.DateTime
                       ), ct)
               + 1;
    }

    public async Task<ulong?> FindTopPlayerDiscordIdAsync(ulong guildId, CancellationToken ct = default)
    {
        return await Set
            .Include(x => x.Player)
            .Where(IsTopScore(guildId))
            .OrderByDescending(x => x.TotalScore)
            .ThenBy(x => x.DateTime)
            .Select(x => x.Player.DiscordUserId)
            .FirstOrDefaultAsync(ct);
    }

    private Expression<Func<DbBeatmapCompetitionScore, bool>> IsTopScore(ulong guildId)
    {
        return score => Set
            .Where(y =>
                y.GuildId == guildId
                && Context
                    .BeatmapCompetitions
                    .Where(z => z.GuildId == guildId)
                    .OrderByDescending(z => z.EndTime)
                    .Take(1)
                    .Select(z => z.StartTime)
                    .Contains(y.StartTime)
            )
            .GroupBy(y => y.PlayerId)
            .Select(y => y.Max(z => z.TotalScore))
            .Contains(score.TotalScore);
    }
}
