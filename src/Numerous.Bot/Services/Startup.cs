// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Numerous.Bot.Discord;
using Numerous.Bot.Discord.Events;
using Numerous.Common.Config;
using Numerous.Database.Context;

namespace Numerous.Bot.Services;

public sealed class Startup(
    DiscordSocketClient discordClient,
    IConfigProvider cfgProvider,
    IUnitOfWorkFactory uowFactory,
    ReminderService reminderService,
    OsuVerifier verifier,
    DiscordEventHandler eventHandler
) : IHostedService
{
    private Config Cfg => cfgProvider.Get();

    public async Task StartAsync(CancellationToken ct)
    {
        discordClient.Log += Log;

        await discordClient.LoginAsync(TokenType.Bot, Cfg.BotToken);
        await discordClient.StartAsync();

        await using var uow = uowFactory.Create();

        foreach (var guild in await discordClient.Rest.GetGuildsAsync())
        {
            if (!await uow.Guilds.ExistsAsync(guild.Id, ct))
            {
                await uow.Guilds.InsertAsync(new()
                {
                    Id = guild.Id,
                }, ct);
            }
        }

        await uow.CommitAsync(ct);

        reminderService.StartAsync(ct);
        await verifier.StartAsync(ct);
        eventHandler.Start();
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
