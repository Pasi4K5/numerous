using MongoDB.Bson.Serialization.Attributes;

namespace Numerous.Database.Entities;

public record DbEntity<TId> where TId : struct
{
    [BsonId]
    public virtual required TId Id { get; init; }
}

public record DbEntity : DbEntity<Guid>
{
    public override required Guid Id { get; init; } = Guid.NewGuid();
}
