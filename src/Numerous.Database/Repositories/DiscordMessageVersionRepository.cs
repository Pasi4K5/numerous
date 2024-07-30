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

public interface IDiscordMessageVersionRepository : IRepository<DiscordMessageVersionDto>;

public sealed class DiscordMessageVersionRepository(NumerousDbContext context, IMapper mapper)
    : Repository<DbDiscordMessageVersion, DiscordMessageVersionDto>(context, mapper), IDiscordMessageVersionRepository
{
    public override async Task InsertAsync(DiscordMessageVersionDto dto, CancellationToken ct = default)
    {
        if (await Context.DiscordMessages.AnyAsync(x => x.Id == dto.MessageId, ct))
        {
            await base.InsertAsync(dto, ct);
        }
    }
}
