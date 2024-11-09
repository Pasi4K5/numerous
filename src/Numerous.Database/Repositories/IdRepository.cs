// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Numerous.Database.Context;
using Numerous.Database.Entities;

namespace Numerous.Database.Repositories;

public interface IIdRepository<TDto, in TId> : IRepository<TDto>
    where TDto : class, IHasId<TId>
    where TId : struct, IEquatable<TId>
{
    /// <summary>
    /// Inserts the given DTO, immediately calls <see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/>,
    /// and sets the ID of the DTO to the ID of the entity.
    /// </summary>
    public Task ExecuteInsertAsync(TDto dto, CancellationToken ct = default);

    Task<TDto?> FindAsync(TId id, CancellationToken ct = default);
    Task<TDto> GetAsync(TId id, CancellationToken ct = default);
    Task<bool> ExistsAsync(TId id, CancellationToken ct = default);
    Task EnsureExistsAsync(TDto dto, CancellationToken ct = default);
    Task DeleteByIdAsync(TId id, CancellationToken ct = default);
}

public class IdRepository<TEntity, TDto, TId>(NumerousDbContext context, IMapper mapper)
    : Repository<TEntity, TDto>(context, mapper), IIdRepository<TDto, TId>
    where TEntity : DbEntity<TId>
    where TDto : class, IHasId<TId>
    where TId : struct, IEquatable<TId>
{
    public async Task ExecuteInsertAsync(TDto dto, CancellationToken ct = default)
    {
        var entity = Mapper.Map<TEntity>(dto);
        await Set.AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);

        dto.Id = entity.Id;
    }

    public async Task<TDto?> FindAsync(TId id, CancellationToken ct = default)
    {
        return Mapper.Map<TDto?>(await Set.FindAsync([id], ct));
    }

    public async Task<TDto> GetAsync(TId id, CancellationToken ct = default)
    {
        return Mapper.Map<TDto>(
            await Set.FindAsync([id], ct)
            ?? throw new InvalidOperationException(
                $"Entity of type {typeof(TEntity).Name} with ID {id} not found."
            )
        );
    }

    public async Task<bool> ExistsAsync(TId id, CancellationToken ct = default)
    {
        return await Set.AnyAsync(x => x.Id.Equals(id), ct);
    }

    public virtual async Task EnsureExistsAsync(TDto dto, CancellationToken ct = default)
    {
        var entity = Mapper.Map<TEntity>(dto);

        if (!await Set.AnyAsync(x => x.Id.Equals(entity.Id), ct))
        {
            await Set.AddAsync(entity, ct);
        }
    }

    public async Task DeleteByIdAsync(TId id, CancellationToken ct = default)
    {
        var entity = await Set.FindAsync([id], ct);

        if (entity is not null)
        {
            Set.Remove(entity);
        }
    }
}
