// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Numerous.Common.Enums;
using Numerous.Database.Context;
using Numerous.Database.Dtos;
using Numerous.Database.Entities;

namespace Numerous.Database.Repositories;

public interface IGroupRoleMappingRepository : IRepository<GroupRoleMappingDto>
{
    Task<GroupRoleMappingDto[]> GetByGuildAsync(ulong guildId, CancellationToken ct = default);
    Task UpsertAsync(GroupRoleMappingDto entity, CancellationToken ct = default);
    Task DeleteAsync(ulong guildId, OsuUserGroup group, CancellationToken ct = default);
}

public sealed class GroupRoleMappingRepository(NumerousDbContext context, IMapper mapper)
    : Repository<DbGroupRoleMapping, GroupRoleMappingDto>(context, mapper), IGroupRoleMappingRepository
{
    public async Task<GroupRoleMappingDto[]> GetByGuildAsync(ulong guildId, CancellationToken ct = default)
    {
        return Mapper.Map<GroupRoleMappingDto[]>(
            await Set
                .Where(x => x.GuildId == guildId)
                .ToArrayAsync(ct)
        );
    }

    public async Task UpsertAsync(GroupRoleMappingDto dto, CancellationToken ct = default)
    {
        var entity = Mapper.Map<DbGroupRoleMapping>(dto);

        var existing = await Set.FirstOrDefaultAsync(
            x =>
                x.GuildId == entity.GuildId
                && x.RoleId == entity.RoleId
                && x.Group == entity.Group,
            ct
        );

        if (existing is not null)
        {
            existing.RoleId = entity.RoleId;
            existing.Group = entity.Group;
        }
        else
        {
            await InsertAsync(dto, ct);
        }
    }

    public async Task DeleteAsync(ulong guildId, OsuUserGroup group, CancellationToken ct = default)
    {
        var mappings = await Set
            .Where(x => x.GuildId == guildId && x.Group == group)
            .ToArrayAsync(ct);

        Context.GroupRoleMappings.RemoveRange(mappings);
    }
}
