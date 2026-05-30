// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using AutoMapper;
using Numerous.Database;
using Numerous.Database.Context;
using Numerous.Database.Dtos;
using Numerous.Database.Repositories;
using TimeZoneInfo = System.TimeZoneInfo;

namespace Numerous.Tests.Repositories;

public sealed class DiscordUserRepositoryTests(TestDatabaseFixture fixture) : RepositoryTestBase(fixture)
{
    private static DiscordUserRepository CreateRepository(NumerousDbContext context)
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<DbMapperProfile>());
        var mapper = configuration.CreateMapper();

        return new DiscordUserRepository(context, mapper);
    }

    [Fact]
    public async Task InsertAsync_ShouldInsertEntity()
    {
        await using var context = CreateDbContextWithTransaction();
        var repository = CreateRepository(context);

        var dto = new DiscordUserDto
        {
            Id = 9999,
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        await repository.InsertAsync(dto);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var entity = await context.DiscordUsers.FindAsync((ulong)9999);
        entity.Should().NotBeNull();
        entity!.Id.Should().Be(9999);
        entity.TimeZoneId.Should().Be("America/Los_Angeles");
    }

    [Fact]
    public async Task InsertManyAsync_ShouldInsertMultipleEntities()
    {
        await using var context = CreateDbContextWithTransaction();
        var repository = CreateRepository(context);

        var dtos = new[]
        {
            new DiscordUserDto { Id = 10001, TimeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC") },
            new DiscordUserDto { Id = 10002, TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris") },
        };

        await repository.InsertManyAsync(dtos);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var entities = context.DiscordUsers.Where(x => x.Id >= 10001).ToList();
        entities.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        await using var context = CreateDbContext();
        var repository = CreateRepository(context);

        var result = await repository.GetAllAsync();

        result.Should().NotBeEmpty();
        result.Should().Contain(x => x.Id == 1001);
    }

    [Fact]
    public async Task ExecuteInsertAsync_ShouldInsertAndSetId()
    {
        await using var context = CreateDbContextWithTransaction();
        var repository = CreateRepository(context);

        var dto = new DiscordUserDto { Id = 9998 };
        dto.Id.Should().Be(9998);

        await repository.ExecuteInsertAsync(dto);

        dto.Id.Should().Be(9998);

        context.ChangeTracker.Clear();
        var entity = await context.DiscordUsers.FindAsync(dto.Id);
        entity.Should().NotBeNull();
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEntity_WhenExists()
    {
        await using var context = CreateDbContext();
        var repository = CreateRepository(context);

        var result = await repository.FindAsync(1001);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1001);
        result.TimeZone!.Id.Should().Be("America/New_York");
    }

    [Fact]
    public async Task FindAsync_ShouldReturnNull_WhenNotExists()
    {
        await using var context = CreateDbContext();
        var repository = CreateRepository(context);

        var result = await repository.FindAsync(999999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnEntity_WhenExists()
    {
        await using var context = CreateDbContext();
        var repository = CreateRepository(context);

        var result = await repository.GetAsync(1001);

        result.Id.Should().Be(1001);
        result.TimeZone!.Id.Should().Be("America/New_York");
    }

    [Fact]
    public async Task GetAsync_ShouldThrow_WhenNotExists()
    {
        await using var context = CreateDbContext();
        var repository = CreateRepository(context);

        var act = () => repository.GetAsync(999999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenExists()
    {
        await using var context = CreateDbContext();
        var repository = CreateRepository(context);

        var result = await repository.ExistsAsync(1001);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenNotExists()
    {
        await using var context = CreateDbContext();
        var repository = CreateRepository(context);

        var result = await repository.ExistsAsync(999999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task EnsureExistsAsync_ShouldDoNothing_WhenExists()
    {
        await using var context = CreateDbContextWithTransaction();
        var repository = CreateRepository(context);

        var dto = new DiscordUserDto { Id = 1001 };
        await repository.EnsureExistsAsync(dto);
        await context.SaveChangesAsync();

        var count = context.DiscordUsers.Local.Count;
        count.Should().Be(0);
    }

    [Fact]
    public async Task EnsureExistsAsync_ShouldInsert_WhenNotExists()
    {
        await using var context = CreateDbContextWithTransaction();
        var repository = CreateRepository(context);

        var dto = new DiscordUserDto { Id = 10099 };
        await repository.EnsureExistsAsync(dto);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var entity = await context.DiscordUsers.FindAsync((ulong)10099);
        entity.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldDeleteEntity()
    {
        await using var context = CreateDbContextWithTransaction();
        var repository = CreateRepository(context);

        var dto = new DiscordUserDto { Id = 1004 };
        await repository.InsertAsync(dto);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        await repository.DeleteByIdAsync(1004);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var entity = await context.DiscordUsers.FindAsync((ulong)1004);
        entity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldDoNothing_WhenNotExists()
    {
        await using var context = CreateDbContextWithTransaction();
        var repository = CreateRepository(context);

        var act = () => repository.DeleteByIdAsync(999999);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetTimezoneAsync_ShouldUpdateTimezone_WhenUserExists()
    {
        await using var context = CreateDbContextWithTransaction();
        var repository = CreateRepository(context);

        var newTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        await repository.SetTimezoneAsync(1001, newTimeZone);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var entity = await context.DiscordUsers.FindAsync((ulong)1001);
        entity!.TimeZoneId.Should().Be("Europe/Berlin");
    }

    [Fact]
    public async Task SetTimezoneAsync_ShouldCreateUser_WhenNotExists()
    {
        await using var context = CreateDbContextWithTransaction();
        var repository = CreateRepository(context);

        var newTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
        await repository.SetTimezoneAsync(10099, newTimeZone);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var entity = await context.DiscordUsers.FindAsync((ulong)10099);
        entity.Should().NotBeNull();
        entity!.TimeZoneId.Should().Be("Asia/Seoul");
    }

    [Fact]
    public async Task SetTimezoneAsync_ShouldClearTimezone_WhenNull()
    {
        await using var context = CreateDbContextWithTransaction();
        var repository = CreateRepository(context);

        await repository.SetTimezoneAsync(1001, null);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var entity = await context.DiscordUsers.FindAsync((ulong)1001);
        entity!.TimeZoneId.Should().BeNull();
    }
}
