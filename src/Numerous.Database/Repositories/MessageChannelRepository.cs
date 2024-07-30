// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Numerous.Database.Context;
using Numerous.Database.Dtos;
using Numerous.Database.Entities;

namespace Numerous.Database.Repositories;

public interface IMessageChannelRepository : IIdRepository<MessageChannelDto, ulong>
{
    Task<bool> IsReadOnlyAsync(ulong channelId, CancellationToken ct = default);
    Task<bool> ToggleReadOnlyAsync(ulong guildId, ulong channelId, CancellationToken ct = default);
}

public sealed class MessageChannelRepository(NumerousDbContext context, IMapper mapper)
    : IdRepository<DbMessageChannel, MessageChannelDto, ulong>(context, mapper), IMessageChannelRepository
{
    public async Task<bool> IsReadOnlyAsync(ulong channelId, CancellationToken ct = default)
    {
        var channel = await Set
            .Select(x => new { x.Id, x.IsReadOnly })
            .FirstOrDefaultAsync(x => x.Id == channelId, ct);

        return channel?.IsReadOnly ?? false;
    }

    public async Task<bool> ToggleReadOnlyAsync(ulong guildId, ulong channelId, CancellationToken ct = default)
    {
        var channel = await Set.FindAsync([channelId], ct);

        if (channel is null)
        {
            await Set.AddAsync(new DbMessageChannel
            {
                Id = channelId,
                IsReadOnly = true,
                GuildId = guildId,
            }, ct);
        }
        else
        {
            channel.IsReadOnly = !channel.IsReadOnly;
        }

        return channel?.IsReadOnly ?? true;
    }
}
