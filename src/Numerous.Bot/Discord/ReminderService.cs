// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Numerous.Database.Context;
using Numerous.Database.Dtos;
using Timer = System.Timers.Timer;

namespace Numerous.Bot.Discord;

public sealed class ReminderService(IHost host, IUnitOfWorkFactory uowFactory, DiscordSocketClient client)
{
    private static readonly TimeSpan _cacheInterval = TimeSpan.FromHours(1) + TimeSpan.FromMinutes(1);

    private readonly Dictionary<uint, Timer> _timerCache = new();

    public void StartAsync(CancellationToken ct)
    {
        host.Services.UseScheduler(s =>
            s.ScheduleAsync(() => CacheRemindersAsync(ct)).Hourly().RunOnceAtStart().PreventOverlapping(nameof(ReminderService))
        );
    }

    public async Task AddReminderAsync(ReminderDto reminder, bool insertIntoDb = true, CancellationToken ct = default)
    {
        if (reminder.Timestamp < DateTimeOffset.Now + _cacheInterval)
        {
            var timer = new Timer
            {
                AutoReset = false,
                Interval = (reminder.Timestamp - DateTimeOffset.Now).TotalMilliseconds,
            };

            _timerCache[reminder.Id] = timer;

            timer.Elapsed += async (_, _) => await TriggerReminderAsync(reminder, ct);

            timer.Start();
        }

        if (insertIntoDb)
        {
            await using var uow = uowFactory.Create();

            await uow.Reminders.InsertAsync(reminder, ct);

            await uow.CommitAsync(ct);
        }
    }

    public async Task RemoveReminderAsync(ReminderDto reminder)
    {
        if (_timerCache.TryGetValue(reminder.Id, out var timer))
        {
            timer.Stop();
            timer.Dispose();
        }

        await using var uow = uowFactory.Create();

        await uow.Reminders.DeleteByIdAsync(reminder.Id);

        await uow.CommitAsync();
    }

    private async Task CacheRemindersAsync(CancellationToken ct)
    {
        await using var uow = uowFactory.Create();

        foreach (var reminder in await uow.Reminders.GetRemindersBeforeAsync(DateTimeOffset.UtcNow + _cacheInterval, ct))
        {
            if (_timerCache.ContainsKey(reminder.Id))
            {
                return;
            }

            if (reminder.Timestamp <= DateTimeOffset.Now)
            {
                await TriggerReminderAsync(reminder, ct);
            }
            else
            {
                await AddReminderAsync(reminder, false, ct);
            }
        }
    }

    private async Task TriggerReminderAsync(ReminderDto reminder, CancellationToken ct)
    {
        if (await client.GetChannelAsync(reminder.ChannelId) is not IMessageChannel channel)
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

        await using var uow = uowFactory.Create();

        await uow.Reminders.DeleteByIdAsync(reminder.Id, ct);

        await uow.CommitAsync(ct);

        _timerCache.Remove(reminder.Id);
    }
}
