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

public interface IAutoPingMappingRepository : IRepository<AutoPingMappingDto>
{
    Task<AutoPingMappingDto[]> GetByChannelIdAsync(ulong channelId, CancellationToken ct = default);
}

public sealed class AutoPingMappingRepository(NumerousDbContext context, IMapper mapper)
    : Repository<DbAutoPingMapping, AutoPingMappingDto>(context, mapper), IAutoPingMappingRepository
{
    public override async Task InsertAsync(AutoPingMappingDto dto, CancellationToken ct = default)
    {
        await EnsureChannelExistsAsync<DbForumChannel>(dto.GuildId, dto.ChannelId, ct);

        var existing = await Set
            .FirstOrDefaultAsync(x => x.ChannelId == dto.ChannelId && x.TagId == dto.TagId, ct);

        if (existing is not null)
        {
            existing.RoleId = dto.RoleId;
        }
        else
        {
            await base.InsertAsync(dto, ct);
        }
    }

    public async Task<AutoPingMappingDto[]> GetByChannelIdAsync(ulong channelId, CancellationToken ct = default)
    {
        return Mapper.Map<AutoPingMappingDto[]>(
            await Set
                .Where(x => x.ChannelId == channelId)
                .ToArrayAsync(ct)
        );
    }
}
