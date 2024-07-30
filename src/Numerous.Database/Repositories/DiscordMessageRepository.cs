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

public interface IDiscordMessageRepository : IIdRepository<DiscordMessageDto, ulong>
{
    Task<DiscordMessageDto?> FindWithVersionsInOrderAsync(ulong id, CancellationToken ct = default);
    Task<List<DiscordMessageDto>> GetOrderedDeletedMessagesWithLastVersionAsync(ulong channelId, CancellationToken ct = default);
    Task SetDeletedAsync(ulong msgId, CancellationToken ct = default);
    Task HideAsync(ulong msgId, CancellationToken ct = default);
}

public sealed class DiscordMessageRepository(NumerousDbContext context, IMapper mapper)
    : IdRepository<DbDiscordMessage, DiscordMessageDto, ulong>(context, mapper), IDiscordMessageRepository
{
    public override async Task InsertAsync(DiscordMessageDto msg, CancellationToken ct = default)
    {
        await EnsureDiscordUserExistsAsync(msg.AuthorId, ct);
        await EnsureChannelExistsAsync<DbMessageChannel>(msg.GuildId, msg.ChannelId, ct);

        await base.InsertAsync(msg, ct);
    }

    public async Task<DiscordMessageDto?> FindWithVersionsInOrderAsync(ulong id, CancellationToken ct = default)
    {
        return Mapper.Map<DiscordMessageDto?>(
            await Set
                .Include(x => x.Versions)
                .Where(x => x.Id == id)
                .OrderBy(x => x.Versions.Min(v => v.Timestamp))
                .FirstOrDefaultAsync(ct)
        );
    }

    public async Task<List<DiscordMessageDto>> GetOrderedDeletedMessagesWithLastVersionAsync(ulong channelId, CancellationToken ct = default)
    {
        return Mapper.Map<List<DiscordMessageDto>>(
            await Set
                .Where(x => x.ChannelId == channelId && x.DeletedAt != null && !x.IsHidden)
                .Include(x => x.Versions.OrderByDescending(v => v.Timestamp).Take(1))
                .OrderBy(x => x.Id)
                .ToListAsync(ct)
        );
    }

    public async Task SetDeletedAsync(ulong msgId, CancellationToken ct = default)
    {
        var msg = await Set.FindAsync([msgId], ct);

        if (msg is not null)
        {
            msg.DeletedAt = DateTime.UtcNow;
        }
    }

    public async Task HideAsync(ulong msgId, CancellationToken ct = default)
    {
        var msg = await Set.FindAsync([msgId], ct);

        if (msg is not null)
        {
            msg.IsHidden = true;
        }
    }
}
