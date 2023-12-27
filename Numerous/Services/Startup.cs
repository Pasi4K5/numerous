using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Numerous.Configuration;
using Numerous.Database;
using Numerous.Database.Entities;
using Numerous.DependencyInjection;

namespace Numerous.Services;

[HostedService]
public sealed class Startup(
    DiscordSocketClient discordClient,
    ConfigManager cfgManager,
    DbManager dbManager
) : IHostedService
{
    private Config Cfg => cfgManager.Get();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        discordClient.Log += Log;

        await discordClient.LoginAsync(TokenType.Bot, Cfg.BotToken);
        await discordClient.StartAsync();

        foreach (var guild in await discordClient.Rest.GetGuildsAsync())
        {
            var guildOptions = await dbManager.GuildOptions.Find(x => x.Id == guild.Id).FirstOrDefaultAsync(cancellationToken);

            if (guildOptions is null)
            {
                await dbManager.GuildOptions.InsertOneAsync(new GuildOptions
                {
                    Id = guild.Id,
                }, cancellationToken: cancellationToken);
            }
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await discordClient.LogoutAsync();
        await discordClient.StopAsync();
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());

        return Task.CompletedTask;
    }
}
