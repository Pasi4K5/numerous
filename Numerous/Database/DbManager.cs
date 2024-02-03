// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using MongoDB.Driver;
using Numerous.Configuration;
using Numerous.Database.Entities;
using Numerous.DependencyInjection;

namespace Numerous.Database;

[SingletonService]
public sealed class DbManager
{
    private readonly IMongoDatabase _db;

    public IMongoCollection<GuildOptions> GuildOptions =>
        _db.GetCollection<GuildOptions>("guildOptions");

    public IMongoCollection<DiscordMessage> DiscordMessages =>
        _db.GetCollection<DiscordMessage>("discordMessages");

    public IMongoCollection<DbUser> Users =>
        _db.GetCollection<DbUser>("users");

    public IMongoCollection<Reminder> Reminders =>
        _db.GetCollection<Reminder>("reminders");

    public IMongoCollection<GuildStats> GuildStats =>
        _db.GetCollection<GuildStats>("guildStats");

    public DbManager(ConfigManager configManager)
    {
        var config = configManager.Get();
        var dbName = config.MongoDatabaseName;

        _db = new MongoClient(config.MongoConnectionString).GetDatabase(dbName);

        if (_db is null)
        {
            throw new Exception($"Failed to get database {dbName}.");
        }
    }

    public async Task<DbUser> GetUserAsync(ulong id)
    {
        var user = await Users.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (user is null)
        {
            user = new DbUser
            {
                Id = id,
            };

            await Users.InsertOneAsync(user);
        }

        return user;
    }
}
