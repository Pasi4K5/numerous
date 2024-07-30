using Microsoft.EntityFrameworkCore;
using Numerous.Database.Entities;

namespace Numerous.Database.Context;

public sealed class NumerousDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<DbAutoPingMapping> AutoPingMappings { get; set; }
    public DbSet<DbChannel> Channels { get; set; }
    public DbSet<DbDiscordMessage> DiscordMessages { get; set; }
    public DbSet<DbDiscordMessageVersion> DiscordMessageVersions { get; set; }
    public DbSet<DbDiscordUser> DiscordUsers { get; set; }
    public DbSet<DbForumChannel> ForumChannels { get; set; }
    public DbSet<DbGroupRoleMapping> GroupRoleMappings { get; set; }
    public DbSet<DbGuild> Guilds { get; set; }
    public DbSet<DbJoinMessage> JoinMessages { get; set; }
    public DbSet<DbMessageChannel> MessageChannels { get; set; }
    public DbSet<DbOsuUser> OsuUsers { get; set; }
    public DbSet<DbReminder> Reminders { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.UseSerialColumns();

        builder.Entity<DbAutoPingMapping>(e =>
        {
            const string idName = "Id";
            e.Property<uint>(idName).ValueGeneratedOnAdd();
            e.HasKey(idName);

            e.HasIndex(
                nameof(DbAutoPingMapping.ChannelId),
                nameof(DbAutoPingMapping.TagId),
                nameof(DbAutoPingMapping.RoleId)
            ).IsUnique();
        });

        builder.Entity<DbGuild>(e =>
        {
            e.HasIndex(nameof(DbGuild.UnverifiedRoleId)).IsUnique();
        });

        builder.Entity<DbJoinMessage>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint(
                "CK_JoinMessage_HasText",
                $"\"{nameof(DbJoinMessage.Title)}\" IS NOT NULL OR \"{nameof(DbJoinMessage.Description)}\" IS NOT NULL"
            ));
        });

        builder.Entity<DbOsuUser>(e =>
        {
            e.HasIndex(nameof(DbOsuUser.DiscordUserId)).IsUnique();
        });
    }
}
