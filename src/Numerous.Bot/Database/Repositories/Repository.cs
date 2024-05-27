// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Linq.Expressions;
using MongoDB.Driver;
using Numerous.Bot.Database.Entities;

namespace Numerous.Bot.Database.Repositories;

public interface IRepository<TEntity, in TId>
    where TEntity : IDbEntity<TId>
    where TId : struct, IEquatable<TId>
{
    Task<TEntity?> FindByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <remarks>The presence of the document with the given <paramref name="id"/> is <b>NOT</b> guaranteed immediately after the method returns.</remarks>
    Task<TEntity> FindOrInsertByIdAsync(TId id, CancellationToken ct = default);

    Task<IAsyncCursor<TEntity>> FindManyAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default);
    Task InsertAsync(TEntity entity, CancellationToken ct = default);
    Task DeleteByIdAsync(TId id, CancellationToken ct = default);
    Task UpdateByIdAsync<TProp>(TId id, Expression<Func<TEntity, TProp>> property, TProp value, CancellationToken ct = default);
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
    public async Task<TEntity> FindOrInsertByIdAsync(TId id, CancellationToken ct = default)
    {
        var entity = await FindByIdAsync(id, ct);

        if (entity is not null)
        {
            return entity;
        }

        var newEntity = await CreateEntityAsync(id);

        Collection.InsertOneAsync(newEntity, cancellationToken: ct).Start();

        return newEntity;
    }

    public Task<IAsyncCursor<TEntity>> FindManyAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default)
    {
        return Collection.FindAsync(filter ?? FilterDefinition<TEntity>.Empty, cancellationToken: ct);
    }

    public Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken ct = default)
    {
        var result = filter is null ? Collection.Find(Builders<TEntity>.Filter.Empty) : Collection.Find(filter);

        return result.AnyAsync(ct);
    }

    public async Task InsertAsync(TEntity entity, CancellationToken ct = default)
    {
        await Collection.InsertOneAsync(entity, cancellationToken: ct);
    }

    public Task DeleteByIdAsync(TId id, CancellationToken ct = default)
    {
        return Collection.DeleteOneAsync(Builders<TEntity>.Filter.Eq(x => x.Id, id), ct);
    }

    public async Task UpdateByIdAsync<TProp>(TId id, Expression<Func<TEntity, TProp>> property, TProp value, CancellationToken ct = default)
    {
        await Collection.UpdateOneAsync(
            Builders<TEntity>.Filter.Eq(x => x.Id, id),
            Builders<TEntity>.Update.Set(property, value),
            cancellationToken: ct
        );
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
