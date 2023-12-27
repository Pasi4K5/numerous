namespace Numerous.Database.Entities;

public sealed record GuildOptions : DbEntity<ulong>
{
    public bool TrackMessages { get; init; }

    public TrackingOptions[] PlayerTrackingOptions { get; init; } = Array.Empty<TrackingOptions>();

    public record struct TrackingOptions
    {
        public ulong DiscordId { get; init; }
        public bool TrackPlayer { get; init; }
    }
}
