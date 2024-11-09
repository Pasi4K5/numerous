using Microsoft.EntityFrameworkCore;
using Numerous.Database.Entities;

namespace Numerous.Database.Context;

public sealed class NumerousDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<DbAutoPingMapping> AutoPingMappings { get; set; }
    public DbSet<DbBeatmapCompetition> BeatmapCompetitions { get; set; }
    public DbSet<DbBeatmapCompetitionScore> BeatmapCompetitionScores { get; set; }
    public DbSet<DbChannel> Channels { get; set; }
    public DbSet<DbDiscordMessage> DiscordMessages { get; set; }
    public DbSet<DbDiscordMessageVersion> DiscordMessageVersions { get; set; }
    public DbSet<DbDiscordUser> DiscordUsers { get; set; }
    public DbSet<DbForumChannel> ForumChannels { get; set; }
    public DbSet<DbGroupRoleMapping> GroupRoleMappings { get; set; }
    public DbSet<DbGuild> Guilds { get; set; }
    public DbSet<DbJoinMessage> JoinMessages { get; set; }
    public DbSet<DbLocalBeatmap> LocalBeatmaps { get; set; }
    public DbSet<DbMessageChannel> MessageChannels { get; set; }
    public DbSet<DbOnlineBeatmap> OnlineBeatmaps { get; set; }
    public DbSet<DbOnlineBeatmapset> OnlineBeatmapsets { get; set; }
    public DbSet<DbOsuUser> OsuUsers { get; set; }
    public DbSet<DbReminder> Reminders { get; set; }
    public DbSet<DbReplay> Replays { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.UseSerialColumns();

        builder.Entity<DbGuild>(e =>
        {
            e.HasMany(x => x.Channels)
                .WithOne(x => x.Guild)
                .HasForeignKey(x => x.GuildId)
                .IsRequired();

            e.HasOne(x => x.MapfeedChannel)
                .WithOne()
                .HasForeignKey<DbGuild>(x => x.MapfeedChannelId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<DbBeatmapCompetition>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint(
                "CK_BeatmapCompetition_ValidTime",
                $"\"{nameof(DbBeatmapCompetition.StartTime)}\" < \"{nameof(DbBeatmapCompetition.EndTime)}\""
            ));
        });

        builder.Entity<DbAutoPingMapping>(e =>
        {
            const string idName = "Id";
            e.Property<uint>(idName).ValueGeneratedOnAdd();
            e.HasKey(idName);
        });

        builder.Entity<DbJoinMessage>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint(
                "CK_JoinMessage_HasText",
                $"\"{nameof(DbJoinMessage.Title)}\" IS NOT NULL OR \"{nameof(DbJoinMessage.Description)}\" IS NOT NULL"
            ));
        });

        builder.Entity<DbLocalBeatmap>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint(
                "CK_LocalBeatmap_ValidSha256",
                $"length(\"{nameof(DbLocalBeatmap.OszHash)}\") = 256 / 8"
            ));
        });
    }
}
