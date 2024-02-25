// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Numerous.Database;
using Numerous.Database.Entities;
using Numerous.DependencyInjection;

namespace Numerous.Discord;

[HostedService]
public class Nush(DiscordSocketClient client, DbManager db) : IHostedService
{
    private const ulong Sudoer = 345885199386804235;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.MessageReceived += HandleMessageAsync;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(SocketMessage msg)
    {
        var content = msg.Content;

        if (!content.StartsWith("sudo ", StringComparison.CurrentCultureIgnoreCase))
        {
            return;
        }

        if (msg.Author.Id != Sudoer)
        {
            await msg.ReplyAsync("`You are not a sudoer.`");

            return;
        }

        var args = content.Split(' ');

        if (args.Length < 2)
        {
            await msg.ReplyAsync("`Please provide a command.`");

            return;
        }

        args = args[1..];

        try
        {
            await HandleCommandAsync(args[0], args[1..], msg);
        }
        catch (Exception e)
        {
            await msg.ReplyAsync($"`Error: {e.Message}`");
        }
    }

    private async Task HandleCommandAsync(string cmd, string[] args, SocketMessage msg)
    {
        switch (cmd)
        {
            case "echo":
                await msg.ReplyAsync(string.Join(' ', args.Select(x => x == "$0" ? "nush" : x)));

                break;
            case "rmmsg":
                if (args.Length < 1)
                {
                    return;
                }

                var msgIds = args[^1].Split(',').Distinct().ToArray();

                if (msgIds.Length == 0)
                {
                    return;
                }

                var found = false;

                if (args.Contains("-f"))
                {
                    var dbMsgs = await db.DiscordMessages.FindAsync(x => msgIds.Contains(x.Id.ToString()));

                    await db.DiscordMessages.UpdateManyAsync(
                        x => msgIds.Contains(x.Id.ToString()),
                        Builders<DiscordMessage>.Update.Set(x => x.IsHidden, true)
                    );

                    found = await dbMsgs.AnyAsync();
                }

                var discordMsgTasks = msgIds.Select(id => msg.Channel.GetMessageAsync(ulong.Parse(id))).ToArray();

                if (discordMsgTasks.Length == 0 && !found)
                {
                    await msg.ReplyAsync("`Message not found.`");
                }
                else
                {
                    await Parallel.ForEachAsync(discordMsgTasks, async (discordMsgTask, _) =>
                    {
                        await (await discordMsgTask).DeleteAsync();
                    });

                    if (discordMsgTasks.Length > 0 || found)
                    {
                        await msg.ReplyAsync("`Success`");
                    }
                }

                break;
            default:
                await msg.ReplyAsync($"`nush: {cmd}: command not found`");

                break;
        }
    }
}
