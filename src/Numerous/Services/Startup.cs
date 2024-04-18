// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Numerous.Configuration;
using Numerous.Database;
using Numerous.Database.Entities;
using Numerous.DependencyInjection;
using Numerous.Discord;

namespace Numerous.Services;

[HostedService]
public sealed class Startup(
    DiscordSocketClient discordClient,
    IConfigService cfgService,
    DbManager dbManager,
    ReminderService reminderService,
    OsuVerifier verifier
) : IHostedService
{
    private Config Cfg => cfgService.Get();

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

        await reminderService.StartAsync(cancellationToken);
        await verifier.StartAsync();
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
