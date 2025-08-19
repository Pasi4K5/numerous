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

public interface IGuildStatsEntryRepository : IRepository<GuildStatsEntryDto>
{
    Task<IDictionary<DateTimeOffset, int>> GetGuildStatsAsync(ulong guildId, CancellationToken ct = default);
}

public sealed class GuildStatsEntryRepository(NumerousDbContext context, IMapper mapper)
    : Repository<DbGuildStatsEntry, GuildStatsEntryDto>(context, mapper), IGuildStatsEntryRepository
{
    public async Task<IDictionary<DateTimeOffset, int>> GetGuildStatsAsync(ulong guildId, CancellationToken ct = default)
    {
        return await Set.Where(x => x.GuildId == guildId)
            .ToDictionaryAsync(x => x.Timestamp.ToDateTimeOffset(), x => x.MemberCount, ct);
    }
}
