// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using MongoDB.Driver;
using Numerous.Database;
using Numerous.Database.Entities;
using Numerous.Util;

namespace Numerous.Discord.Commands;

[UsedImplicitly]
[Group("reminder", "Reminds you.")]
public sealed class ReminderCommandModule(ReminderService reminderService, DbManager db) : CommandModule
{
    [UsedImplicitly]
    [SlashCommand("set", "Sets a reminder.")]
    public async Task SetCommand(
        [Summary("hours")] int? hours = null,
        [Summary("minutes")] int? minutes = null,
        [Summary("seconds")] int? seconds = null,
        [Summary("about")] string? message = null
    )
    {
        if (hours is null && minutes is null && seconds is null)
        {
            await RespondAsync("Please specify a time.");

            return;
        }

        var timestamp = Context.Interaction.CreatedAt;

        if (hours is not null)
        {
            timestamp = timestamp.AddHours(hours.Value);
        }

        if (minutes is not null)
        {
            timestamp = timestamp.AddMinutes(minutes.Value);
        }

        if (seconds is not null)
        {
            timestamp = timestamp.AddSeconds(seconds.Value);
        }

        if (timestamp < Context.Interaction.CreatedAt.AddSeconds(10))
        {
            await RespondAsync("The specified time must be at least 10 seconds in the future.");

            return;
        }

        await DeferAsync();

        await reminderService.AddReminderAsync(new Reminder(Context.User.Id, Context.Channel.Id, timestamp, message));

        await FollowupAsync(embed:
            new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("Reminder Set")
                .WithDescription(
                    (message is not null ? $"{message}\n" : "")
                    + $"{timestamp.ToDiscordTimestampDateTime()} ({timestamp.ToDiscordTimestampRel()})"
                )
                .Build()
        );
    }

    [UsedImplicitly]
    [SlashCommand("list", "Lists your reminders.")]
    public async Task ListCommand()
    {
        await DeferAsync();

        var reminders = await GetOrderedRemindersByUserId();

        var embed = new EmbedBuilder()
            .WithTitle("Your Reminders")
            .WithColor(Color.Blue);

        if (reminders.Count <= 0)
        {
            await FollowupAsync(embed: embed.WithDescription("You don't have any reminders.").Build());

            return;
        }

        foreach (var (reminder, index) in reminders.WithIndexes())
        {
            embed.AddField(
                $"Reminder {index + 1} in <#{reminder.ChannelId}>",
                (reminder.Message is not null ? $"{reminder.Message}\n" : "")
                + $"{reminder.Timestamp.ToDiscordTimestampDateTime()} ({reminder.Timestamp.ToDiscordTimestampRel()})"
            );
        }

        await FollowupAsync(embed: embed.Build());
    }

    [UsedImplicitly]
    [SlashCommand("remove", "Removes a reminder.")]
    public async Task RemoveCommand(
        [Summary("index", "The index of the reminder to remove.")]
        int index
    )
    {
        await DeferAsync();

        var reminders = await GetOrderedRemindersByUserId();

        if (index < 1 || index > reminders.Count)
        {
            await FollowupAsync(embed:
                new EmbedBuilder()
                    .WithColor(Color.DarkRed)
                    .WithDescription("There is no reminder with that index.")
                    .Build()
            );

            return;
        }

        var reminder = reminders[index - 1];

        await reminderService.RemoveReminderAsync(reminder);

        await FollowupAsync(embed:
            new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Reminder Removed")
                .WithDescription(
                    (reminder.Message is not null ? $"{reminder.Message}\n" : "")
                    + $"{reminder.Timestamp.ToDiscordTimestampDateTime()} ({reminder.Timestamp.ToDiscordTimestampRel()})"
                )
                .Build()
        );
    }

    private async Task<List<Reminder>> GetOrderedRemindersByUserId()
    {
        return (await (await db.Reminders
                    .FindAsync(x => x.UserId == Context.User.Id)
                ).ToListAsync()
            ).OrderBy(x => x.Timestamp)
            .ToList();
    }
}
