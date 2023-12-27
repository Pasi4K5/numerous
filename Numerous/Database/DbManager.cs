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
}
