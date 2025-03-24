// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NodaTime.Extensions;
using Numerous.Database.Context;
using Numerous.Database.Dtos;
using Numerous.Database.Entities;

namespace Numerous.Database.Repositories;

public interface IReminderRepository : IIdRepository<ReminderDto, int>
{
    Task<ReminderDto[]> GetRemindersBeforeAsync(DateTimeOffset timestamp, CancellationToken ct = default);
    Task<ReminderDto[]> GetOrderedRemindersAsync(ulong discordUserId, CancellationToken ct = default);
}

public sealed class ReminderRepository(NumerousDbContext context, IMapper mapper)
    : IdRepository<DbReminder, ReminderDto, int>(context, mapper), IReminderRepository
{
    public override async Task InsertAsync(ReminderDto dto, CancellationToken ct = default)
    {
        await EnsureDiscordUserExistsAsync(dto.UserId, ct);

        if (dto.GuildId is not null && dto.ChannelId is not null)
        {
            await EnsureChannelExistsAsync<DbMessageChannel>(dto.GuildId.Value, dto.ChannelId.Value, ct);
        }

        dto.Timestamp = dto.Timestamp.ToUniversalTime();

        await base.InsertAsync(dto, ct);
    }

    public async Task<ReminderDto[]> GetRemindersBeforeAsync(DateTimeOffset timestamp, CancellationToken ct = default)
    {
        return Mapper.Map<ReminderDto[]>(
            await Set
                .Where(x => x.Timestamp < timestamp.ToInstant())
                .ToArrayAsync(ct)
        );
    }

    public async Task<ReminderDto[]> GetOrderedRemindersAsync(ulong discordUserId, CancellationToken ct = default)
    {
        return Mapper.Map<ReminderDto[]>(
            await Set
                .Where(x => x.UserId == discordUserId)
                .OrderBy(x => x.Timestamp)
                .ToArrayAsync(ct)
        );
    }
}
