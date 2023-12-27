namespace Numerous.Database.Entities;

public sealed record DbUser : DbEntity<ulong>
{
    public DbOsuUser? OsuUser { get; init; }
}
