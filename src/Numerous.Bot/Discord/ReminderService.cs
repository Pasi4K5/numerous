// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Numerous.Bot.Database;
using Numerous.Bot.Database.Entities;
using Numerous.Bot.DependencyInjection;
using Timer = System.Timers.Timer;

namespace Numerous.Bot.Discord;

[SingletonService]
public sealed class ReminderService(IHost host, IDbService db, DiscordSocketClient client)
{
    private static readonly TimeSpan _cacheInterval = TimeSpan.FromHours(1) + TimeSpan.FromMinutes(1);

    private readonly Dictionary<Guid, Timer> _timerCache = new();

    public void StartAsync()
    {
        host.Services.UseScheduler(s =>
            s.ScheduleAsync(CacheRemindersAsync).EveryTenSeconds().RunOnceAtStart().PreventOverlapping(nameof(ReminderService))
        );
    }

    public async Task AddReminderAsync(Reminder reminder, bool insertIntoDb = true)
    {
        if (reminder.Timestamp < DateTimeOffset.Now + _cacheInterval)
        {
            var timer = new Timer
            {
                AutoReset = false,
                Interval = (reminder.Timestamp - DateTimeOffset.Now).TotalMilliseconds,
            };

            _timerCache[reminder.Id] = timer;

            timer.Elapsed += async (_, _) => await TriggerReminderAsync(reminder);

            timer.Start();
        }

        if (insertIntoDb)
        {
            await db.Reminders.InsertAsync(reminder);
        }
    }

    public async Task RemoveReminderAsync(Reminder reminder)
    {
        if (_timerCache.TryGetValue(reminder.Id, out var timer))
        {
            timer.Stop();
            timer.Dispose();
        }

        await db.Reminders.DeleteByIdAsync(reminder.Id);
    }

    private async Task CacheRemindersAsync()
    {
        await (await db.Reminders.FindManyAsync()).ForEachAsync(async reminder =>
        {
            if (reminder.Timestamp > DateTimeOffset.Now + _cacheInterval
                || _timerCache.ContainsKey(reminder.Id))
            {
                return;
            }

            if (reminder.Timestamp < DateTimeOffset.Now)
            {
                await TriggerReminderAsync(reminder);
            }
            else
            {
                await AddReminderAsync(reminder, false);
            }
        });
    }

    private async Task TriggerReminderAsync(Reminder reminder)
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

        await db.Reminders.DeleteByIdAsync(reminder.Id);

        _timerCache.Remove(reminder.Id);
    }
}
