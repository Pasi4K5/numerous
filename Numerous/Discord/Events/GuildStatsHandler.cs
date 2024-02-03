// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Numerous.Database;
using Numerous.Database.Entities;
using Numerous.DependencyInjection;

namespace Numerous.Discord.Events;

[HostedService]
public class GuildStatsHandler(IHost host, DiscordSocketClient client, DbManager db) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        host.Services.UseScheduler(scheduler => scheduler.ScheduleAsync(async () =>
        {
            var guildOptions = db.GuildOptions.Find(x => x.TrackMemberCount);
            var now = DateTimeOffset.UtcNow;

            await guildOptions.ForEachAsync(async options =>
            {
                var id = options.Id;

                var guild = client.GetGuild(id);
                var filter = Builders<GuildStats>.Filter.Eq(x => x.Id, id);

                if (!await (await db.GuildStats.FindAsync(filter, cancellationToken: cancellationToken)).AnyAsync(cancellationToken: cancellationToken))
                {
                    var stats = new GuildStats([new GuildStats.Entry<int>(now, guild.MemberCount)])
                    {
                        Id = id,
                    };

                    await db.GuildStats.InsertOneAsync(stats, cancellationToken: cancellationToken);
                }
                else
                {
                    await db.GuildStats.FindOneAndUpdateAsync(
                        filter,
                        Builders<GuildStats>.Update.AddToSet(
                            s => s.MemberCounts,
                            new GuildStats.Entry<int>(now, guild.MemberCount)
                        ), cancellationToken: cancellationToken);
                }
            }, cancellationToken: cancellationToken);
        }).HourlyAt(0));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
