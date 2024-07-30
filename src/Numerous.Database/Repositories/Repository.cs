// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Numerous.Database.Context;
using Numerous.Database.Entities;

namespace Numerous.Database.Repositories;

public interface IRepository<TDto>
    where TDto : class
{
    Task InsertAsync(TDto dto, CancellationToken ct = default);
    Task<TDto[]> GetAllAsync(CancellationToken ct = default);
}

public class Repository<TEntity, TDto>(NumerousDbContext context, IMapper mapper) : IRepository<TDto>
    where TEntity : class
    where TDto : class
{
    protected NumerousDbContext Context { get; } = context;
    protected DbSet<TEntity> Set => Context.Set<TEntity>();
    protected IMapper Mapper { get; } = mapper;

    public virtual async Task InsertAsync(TDto dto, CancellationToken ct = default)
    {
        await Set.AddAsync(Mapper.Map<TEntity>(dto), ct);
    }

    public async Task<TDto[]> GetAllAsync(CancellationToken ct = default)
    {
        return Mapper.Map<TDto[]>(await Set.ToArrayAsync(ct));
    }

    protected async Task<DbDiscordUser> EnsureDiscordUserExistsAsync(ulong userId, CancellationToken ct)
    {
        var user = await Context.DiscordUsers.FindAsync([userId], ct);

        if (user is null)
        {
            user = new DbDiscordUser { Id = userId };
            await Context.DiscordUsers.AddAsync(user, ct);
        }

        return user;
    }

    protected async Task EnsureChannelExistsAsync<TChannel>(ulong guildId, ulong channelId, CancellationToken ct)
        where TChannel : DbChannel, new()
    {
        if (!await Context.Set<TChannel>().AnyAsync(x => x.Id == channelId, ct))
        {
            await Context.Set<TChannel>().AddAsync(new TChannel { Id = channelId, GuildId = guildId }, ct);
        }
    }
}
