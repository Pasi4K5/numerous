// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.EntityFrameworkCore;
using NodaTime;
using Numerous.Common.Enums;
using Numerous.Database.Context;
using Numerous.Database.Entities;
using Testcontainers.PostgreSql;

namespace Numerous.Tests.Repositories;

public sealed class TestDatabaseFixture
{
    private static readonly Lock Lock = new();
    private static bool _databaseInitialized;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    private string ConnectionString => _postgres.GetConnectionString();

    public TestDatabaseFixture()
    {
        lock (Lock)
        {
            if (_databaseInitialized)
            {
                return;
            }

            _postgres.StartAsync().GetAwaiter().GetResult();
            InitializeDatabaseAsync().GetAwaiter().GetResult();
            _databaseInitialized = true;
        }
    }

    private Task InitializeDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<NumerousDbContext>()
            .UseNpgsql(ConnectionString, o => o.UseNodaTime())
            .Options;

        using var context = new NumerousDbContext(options);
        context.Database.Migrate();
        SeedDatabase(context);
        context.SaveChanges();

        return Task.CompletedTask;
    }

    private static void SeedDatabase(NumerousDbContext context)
    {
        var now = SystemClock.Instance.GetCurrentInstant();

        var discordUser1 = new DbDiscordUser
        {
            Id = 1001,
            TimeZoneId = "America/New_York",
        };
        var discordUser2 = new DbDiscordUser
        {
            Id = 1002,
            TimeZoneId = "Europe/London",
        };
        var discordUser3 = new DbDiscordUser
        {
            Id = 1003,
            TimeZoneId = "Asia/Tokyo",
        };
        context.DiscordUsers.AddRange(discordUser1, discordUser2, discordUser3);

        var osuUser1 = new DbOsuUser
        {
            Id = 1,
            DiscordUserId = 1001,
        };
        var osuUser2 = new DbOsuUser
        {
            Id = 2,
            DiscordUserId = 1002,
        };
        var osuUser3 = new DbOsuUser
        {
            Id = 3,
            DiscordUserId = 1003,
        };
        context.OsuUsers.AddRange(osuUser1, osuUser2, osuUser3);

        discordUser1.OsuUser = osuUser1;
        discordUser2.OsuUser = osuUser2;
        discordUser3.OsuUser = osuUser3;

        var guild1 = new DbGuild
        {
            Id = 2001,
            TrackMessages = false,
            VerifiedRoleId = 3001,
        };
        var guild2 = new DbGuild
        {
            Id = 2002,
            TrackMessages = true,
            VerifiedRoleId = 3002,
        };
        context.Guilds.AddRange(guild1, guild2);

        var channel1 = new DbMessageChannel
        {
            Id = 4001,
            GuildId = 2001,
            IsReadOnly = false,
        };
        var channel2 = new DbMessageChannel
        {
            Id = 4002,
            GuildId = 2001,
            IsReadOnly = true,
        };
        var channel3 = new DbMessageChannel
        {
            Id = 4003,
            GuildId = 2002,
            IsReadOnly = false,
        };
        context.Channels.AddRange(channel1, channel2, channel3);

        var forumChannel1 = new DbForumChannel
        {
            Id = 5001,
            GuildId = 2001,
        };
        var forumChannel2 = new DbForumChannel
        {
            Id = 5002,
            GuildId = 2002,
        };
        context.Channels.AddRange(forumChannel1, forumChannel2);

        guild1.Channels = [channel1, channel2, forumChannel1];
        guild2.Channels = [channel3, forumChannel2];

        var groupRoleMappings = new[]
        {
            new DbGroupRoleMapping { GuildId = 2001, RoleId = 4001, Group = OsuUserGroup.RankedMapper },
            new DbGroupRoleMapping { GuildId = 2001, RoleId = 4002, Group = OsuUserGroup.GlobalModerationTeam },
            new DbGroupRoleMapping { GuildId = 2002, RoleId = 4003, Group = OsuUserGroup.BeatmapNominators },
        };
        context.GroupRoleMappings.AddRange(groupRoleMappings);

        var reminder1 = new DbReminder
        {
            Id = 1,
            Timestamp = now.Plus(Duration.FromDays(1)),
            Message = "Test reminder 1",
            UserId = 1001,
            ChannelId = 4001,
        };
        var reminder2 = new DbReminder
        {
            Id = 2,
            Timestamp = now.Plus(Duration.FromDays(7)),
            Message = "Test reminder 2",
            UserId = 1002,
            ChannelId = 4002,
        };
        var reminder3 = new DbReminder
        {
            Id = 3,
            Timestamp = now.Minus(Duration.FromDays(1)),
            Message = "Past reminder",
            UserId = 1003,
        };
        context.Reminders.AddRange(reminder1, reminder2, reminder3);

        var autoPingMapping = new DbAutoPingMapping
        {
            ChannelId = 5001,
            TagId = 6001,
            RoleId = 6002,
        };
        context.AutoPingMappings.Add(autoPingMapping);
    }

    public NumerousDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NumerousDbContext>()
            .UseNpgsql(ConnectionString, o => o.UseNodaTime())
            .Options;

        return new NumerousDbContext(options);
    }

    public NumerousDbContext CreateContextWithTransaction()
    {
        var context = CreateContext();
        context.Database.BeginTransaction();

        return context;
    }
}
