// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using MongoDB.Driver;
using Numerous.Database.Entities;

namespace Numerous.Database.Repositories;

public interface IUserRepository : IRepository<DbUser, ulong>
{
    Task SetTimezoneAsync(ulong id, TimeZoneInfo? timeZone, CancellationToken cancellationToken = default);
    Task SetVerifiedAsync(ulong id, uint osuUserId, CancellationToken cancellationToken = default);
}

public sealed class UserRepository(IMongoDatabase db, string collectionName)
    : Repository<DbUser, ulong>(db, collectionName), IUserRepository
{
    public async Task SetTimezoneAsync(ulong id, TimeZoneInfo? timeZone, CancellationToken cancellationToken)
    {
        await Collection.UpdateOneAsync(
            x => x.Id == id,
            Builders<DbUser>.Update.Set(x => x.TimeZone, timeZone?.Id),
            cancellationToken: cancellationToken
        );
    }

    public async Task SetVerifiedAsync(ulong id, uint osuUserId, CancellationToken cancellationToken)
    {
        await Collection.UpdateOneAsync(
            x => x.Id == id,
            Builders<DbUser>.Update
                .Set(x => x.OsuId, osuUserId),
            options: new() { IsUpsert = true },
            cancellationToken: cancellationToken
        );
    }
}
