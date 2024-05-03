// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using MongoDB.Driver;
using Numerous.Bot.Database.Entities;

namespace Numerous.Bot.Database.Repositories;

public interface IDiscordMessageRepository : IRepository<DiscordMessage, ulong>
{
    Task SetHiddenAsync(ulong messageId, bool hidden = true, CancellationToken cancellationToken = default);
    Task SetDeleted(ulong id, CancellationToken cancellationToken = default);
    Task AddVersionAsync(ulong id, string content, string cleanContent, CancellationToken cancellationToken = default);
}

public sealed class DiscordMessageRepository(IMongoDatabase db, string collectionName)
    : Repository<DiscordMessage, ulong>(db, collectionName), IDiscordMessageRepository
{
    public async Task SetHiddenAsync(ulong messageId, bool hidden = true, CancellationToken cancellationToken = default)
    {
        await Collection.UpdateOneAsync(
            x => x.Id == messageId,
            Builders<DiscordMessage>.Update.Set(x => x.IsHidden, hidden),
            cancellationToken: cancellationToken
        );
    }

    public async Task SetDeleted(ulong id, CancellationToken cancellationToken = default)
    {
        await Collection.UpdateOneAsync(
            x => x.Id == id,
            Builders<DiscordMessage>.Update.Set(x => x.DeletedAt, DateTime.UtcNow),
            cancellationToken: cancellationToken
        );
    }

    public async Task AddVersionAsync(ulong id, string content, string cleanContent, CancellationToken cancellationToken = default)
    {
        await Collection.UpdateOneAsync(
            x => x.Id == id,
            Builders<DiscordMessage>.Update
                .AddToSet(m => m.Timestamps, DateTime.UtcNow)
                .AddToSet(m => m.Contents, content)
                .AddToSet(m => m.CleanContents, cleanContent),
            cancellationToken: cancellationToken
        );
    }
}
