namespace Numerous.Database.Entities;

public sealed record DbOsuUser
{
    public uint Id { get; init; }
    public ulong? LastScoreId { get; init; }
}
