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
        Action<Exception?, string> exLog = msg.Severity switch
        {
            LogSeverity.Critical => Serilog.Log.Fatal,
            LogSeverity.Error => Serilog.Log.Error,
            LogSeverity.Warning => Serilog.Log.Warning,
            LogSeverity.Info => Serilog.Log.Information,
            LogSeverity.Verbose => Serilog.Log.Verbose,
            LogSeverity.Debug => Serilog.Log.Debug,
            _ => throw new ArgumentOutOfRangeException(),
        };

        Action<string, string> msgLog = msg.Severity switch
        {
            LogSeverity.Critical => Serilog.Log.Fatal,
            LogSeverity.Error => Serilog.Log.Error,
            LogSeverity.Warning => Serilog.Log.Warning,
            LogSeverity.Info => Serilog.Log.Information,
            LogSeverity.Verbose => Serilog.Log.Verbose,
            LogSeverity.Debug => Serilog.Log.Debug,
            _ => throw new ArgumentOutOfRangeException(),
        };

        msgLog(msg.Message, "");

        if (msg.Exception is not null)
        {
            exLog(msg.Exception, "Exception");
        }

        return Task.CompletedTask;
    }
}
