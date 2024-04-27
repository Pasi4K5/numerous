// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Linq.Expressions;
using MongoDB.Driver;
using Numerous.Database.Entities;

namespace Numerous.Database.Repositories;

public interface IRepository<TEntity, in TId>
    where TEntity : IDbEntity<TId>
    where TId : struct, IEquatable<TId>
{
    Task<TEntity?> FindByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <remarks>The presence of the document with the given <paramref name="id"/> is <b>NOT</b> guaranteed immediately after the method returns.</remarks>
    Task<TEntity> FindOrInsertByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task<IAsyncCursor<TEntity>> FindManyAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default);
    Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default);
}

public class Repository<TEntity, TId>(IMongoDatabase db, string collectionName) : IRepository<TEntity, TId>
    where TEntity : IDbEntity<TId>, new()
    where TId : struct, IEquatable<TId>
{
    protected readonly IMongoCollection<TEntity> Collection = db.GetCollection<TEntity>(collectionName);

    public async Task<TEntity?> FindByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await Collection
            .Find(x => x.Id.Equals(id))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity> FindOrInsertByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await FindByIdAsync(id, cancellationToken);

        if (entity is not null)
        {
            return entity;
        }

        var newEntity = await CreateEntityAsync(id);

        Collection.InsertOneAsync(newEntity, cancellationToken: cancellationToken).Start();

        return newEntity;
    }

    public Task<IAsyncCursor<TEntity>> FindManyAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
    {
        return Collection.FindAsync(filter ?? FilterDefinition<TEntity>.Empty, cancellationToken: cancellationToken);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
    {
        var result = filter is null ? Collection.Find(Builders<TEntity>.Filter.Empty) : Collection.Find(filter);

        return result.AnyAsync(cancellationToken);
    }

    public async Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
    }

    public Task DeleteByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return Collection.DeleteOneAsync(Builders<TEntity>.Filter.Eq(x => x.Id, id), cancellationToken);
    }

    // Can be made protected, non-static and virtual so that it can be overridden in derived classes.
    protected Task<TEntity> CreateEntityAsync(TId id)
    {
        return Task.FromResult(new TEntity
        {
            Id = id,
        });
    }
}
