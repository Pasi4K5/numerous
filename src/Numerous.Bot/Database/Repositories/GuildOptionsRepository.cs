﻿// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using MongoDB.Driver;
using Numerous.Bot.Database.Entities;

namespace Numerous.Bot.Database.Repositories;

public interface IGuildOptionsRepository : IRepository<GuildOptions, ulong>
{
    Task<bool> ToggleReadOnlyAsync(ulong id, ulong channelId, CancellationToken cancellationToken = default);
    Task UpdateRolesAsync(ulong id, ICollection<GuildOptions.OsuRole> roles, CancellationToken cancellationToken = default);
    Task SetDeletedMessagesChannel(ulong id, ulong? channelId, CancellationToken cancellationToken = default);
}

public sealed class GuildOptionsRepository(IMongoDatabase db, string collectionName)
    : Repository<GuildOptions, ulong>(db, collectionName), IGuildOptionsRepository
{
    /// <returns><see langword="true"/> if the channel is now read-only, <see langword="false"/> otherwise.</returns>
    public async Task<bool> ToggleReadOnlyAsync(ulong id, ulong channelId, CancellationToken cancellationToken = default)
    {
        var guildOptions = await Collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (guildOptions.ReadOnlyChannels.Contains(channelId))
        {
            await Collection.UpdateOneAsync(
                x => x.Id == id,
                Builders<GuildOptions>.Update.Pull(x => x.ReadOnlyChannels, channelId),
                cancellationToken: cancellationToken
            );

            return false;
        }
        else
        {
            await Collection.UpdateOneAsync(
                x => x.Id == id,
                Builders<GuildOptions>.Update.Push(x => x.ReadOnlyChannels, channelId),
                cancellationToken: cancellationToken
            );

            return true;
        }
    }

    public async Task UpdateRolesAsync(ulong id, ICollection<GuildOptions.OsuRole> roles, CancellationToken cancellationToken = default)
    {
        await Collection.UpdateOneAsync(
            Builders<GuildOptions>.Filter.Eq(x => x.Id, id),
            Builders<GuildOptions>.Update.Set(x => x.OsuRoles, roles),
            cancellationToken: cancellationToken
        );
    }

    public async Task SetDeletedMessagesChannel(ulong id, ulong? channelId, CancellationToken cancellationToken = default)
    {
        await Collection.UpdateOneAsync(
            x => x.Id == id,
            Builders<GuildOptions>.Update.Set(x => x.DeletedMessagesChannel, channelId),
            cancellationToken: cancellationToken
        );
    }
}
