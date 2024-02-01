// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;
using JetBrains.Annotations;
using MongoDB.Driver;
using Numerous.Database;
using Numerous.Database.Entities;
using Numerous.DependencyInjection;
using Timer = System.Timers.Timer;

namespace Numerous.Discord;

[SingletonService]
public sealed class ReminderService(DbManager db, DiscordSocketClient client)
{
    [UsedImplicitly] private readonly Dictionary<Guid, Timer> _timers = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var reminders = await db.Reminders
            .Find(FilterDefinition<Reminder>.Empty)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var reminder in reminders)
        {
            if (reminder.Timestamp < DateTimeOffset.Now)
            {
                await db.Reminders.DeleteOneAsync(x => x.Id == reminder.Id, cancellationToken: cancellationToken);
            }
            else
            {
                await AddReminderAsync(reminder, false);
            }
        }
    }

    public async Task AddReminderAsync(Reminder reminder, bool insertIntoDb = true)
    {
        var timer = new Timer
        {
            AutoReset = false,
            Interval = (reminder.Timestamp - DateTimeOffset.Now).TotalMilliseconds,
        };

        _timers[reminder.Id] = timer;

        timer.Elapsed += async (_, _) =>
        {
            var channel = client.GetChannel(reminder.ChannelId) as IMessageChannel;

            if (channel is null)
            {
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(Color.Gold)
                .WithTitle(":alarm_clock: Reminder! :alarm_clock:")
                .WithDescription(reminder.Message ?? "")
                .WithTimestamp(reminder.Timestamp)
                .Build();

            await channel.SendMessageAsync($"<@{reminder.UserId}>", embed: embed);

            await db.Reminders.DeleteOneAsync(x => x.Id == reminder.Id);

            _timers.Remove(reminder.Id);
        };

        timer.Start();

        if (insertIntoDb)
        {
            await db.Reminders.InsertOneAsync(reminder);
        }
    }

    public async Task RemoveReminderAsync(Reminder reminder)
    {
        if (_timers.TryGetValue(reminder.Id, out var timer))
        {
            timer.Stop();
            timer.Dispose();
        }

        await db.Reminders.DeleteOneAsync(x => x.Id == reminder.Id);
    }
}
