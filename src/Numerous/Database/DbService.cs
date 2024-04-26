// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using MongoDB.Driver;
using Numerous.Configuration;
using Numerous.Database.Entities;
using Numerous.Database.Repositories;
using Numerous.DependencyInjection;

namespace Numerous.Database;

public interface IDbService
{
    IGuildOptionsRepository GuildOptions { get; }
    IDiscordMessageRepository DiscordMessages { get; }
    IUserRepository Users { get; }
    IRepository<Reminder, Guid> Reminders { get; }
}

[SingletonService<IDbService>]
public sealed class DbService : IDbService
{
    private readonly IMongoDatabase _db;

    public IGuildOptionsRepository GuildOptions => new GuildOptionsRepository(_db, "guildOptions");
    public IDiscordMessageRepository DiscordMessages => new DiscordMessageRepository(_db, "discordMessages");
    public IUserRepository Users => new UserRepository(_db, "users");
    public IRepository<Reminder, Guid> Reminders => new Repository<Reminder, Guid>(_db, "reminders");

    public DbService(IConfigService configManager)
    {
        var config = configManager.Get();
        var dbName = config.MongoDatabaseName;

        _db = new MongoClient(config.MongoConnectionString).GetDatabase(dbName);

        if (_db is null)
        {
            throw new Exception($"Failed to get database {dbName}.");
        }
    }
}
