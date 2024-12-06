// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Numerous.Database.Dtos;
using Numerous.Database.Entities;
using Numerous.Database.Repositories;

namespace Numerous.Database.Context;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IAutoPingMappingRepository AutoPingMappings { get; }
    IBeatmapCompetitionRepository BeatmapCompetitions { get; }
    IBeatmapCompetitionScoreRepository BeatmapCompetitionScores { get; }
    IDiscordMessageRepository DiscordMessages { get; }
    IDiscordMessageVersionRepository DiscordMessageVersions { get; }
    IDiscordUserRepository DiscordUsers { get; }
    IGroupRoleMappingRepository GroupRoleMappings { get; }
    IGuildRepository Guilds { get; }
    IGuildStatsEntryRepository GuildStats { get; }
    IIdRepository<LocalBeatmapDto, Guid> LocalBeatmaps { get; }
    IIdRepository<JoinMessageDto, ulong> JoinMessages { get; }
    IOnlineBeatmapRepository OnlineBeatmaps { get; }
    IOnlineBeatmapsetRepository OnlineBeatmapsets { get; }
    IMessageChannelRepository MessageChannels { get; }
    IOsuUserRepository OsuUsers { get; }
    IReminderRepository Reminders { get; }

    Task CommitAsync(CancellationToken ct = default);
}

public sealed class UnitOfWork(IDbContextFactory<NumerousDbContext> contextProvider, IMapper mapper) : IUnitOfWork
{
    public IAutoPingMappingRepository AutoPingMappings => new AutoPingMappingRepository(_context, mapper);
    public IBeatmapCompetitionRepository BeatmapCompetitions => new BeatmapCompetitionRepository(_context, mapper);
    public IBeatmapCompetitionScoreRepository BeatmapCompetitionScores => new BeatmapCompetitionScoreRepository(_context, mapper);
    public IDiscordMessageRepository DiscordMessages => new DiscordMessageRepository(_context, mapper);

    public IDiscordMessageVersionRepository DiscordMessageVersions =>
        new DiscordMessageVersionRepository(_context, mapper);

    public IDiscordUserRepository DiscordUsers => new DiscordUserRepository(_context, mapper);
    public IGroupRoleMappingRepository GroupRoleMappings => new GroupRoleMappingRepository(_context, mapper);
    public IGuildRepository Guilds => new GuildRepository(_context, mapper);
    public IGuildStatsEntryRepository GuildStats => new GuildStatsEntryRepository(_context, mapper);
    public IIdRepository<LocalBeatmapDto, Guid> LocalBeatmaps => new IdRepository<DbLocalBeatmap, LocalBeatmapDto, Guid>(_context, mapper);
    public IIdRepository<JoinMessageDto, ulong> JoinMessages => new IdRepository<DbJoinMessage, JoinMessageDto, ulong>(_context, mapper);
    public IOnlineBeatmapRepository OnlineBeatmaps => new OnlineBeatmapRepository(_context, mapper);
    public IOnlineBeatmapsetRepository OnlineBeatmapsets => new OnlineBeatmapsetRepository(_context, mapper);
    public IMessageChannelRepository MessageChannels => new MessageChannelRepository(_context, mapper);
    public IOsuUserRepository OsuUsers => new OsuUserRepository(_context, mapper);
    public IReminderRepository Reminders => new ReminderRepository(_context, mapper);

    private readonly NumerousDbContext _context = contextProvider.CreateDbContext();

    ~UnitOfWork()
    {
        Dispose();
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        _context.Dispose();

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}
