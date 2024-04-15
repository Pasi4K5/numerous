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
    [Group("set", "Sets a reminder.")]
    public sealed class Set(ReminderService reminderService, DbManager db) : CommandModule
    {
        [UsedImplicitly]
        [SlashCommand("in", "Sets a reminder after the specified time.")]
        public async Task InCommand(
            [Summary("hours")] int? hours = null,
            [Summary("minutes")] int? minutes = null,
            [Summary("seconds")] int? seconds = null,
            [Summary("about")] string? message = null
        )
        {
            if (hours is null && minutes is null && seconds is null)
            {
                await RespondWithEmbedAsync("Please specify a time.", type: ResponseType.Error);

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
                        $"{message}\n".OnlyIf(message is not null)
                        + $"{timestamp.ToDiscordTimestampLong()} ({timestamp.ToDiscordTimestampRel()})"
                    )
                    .Build()
            );
        }

        [UsedImplicitly]
        [SlashCommand("at", "Sets a reminder at the specified time.")]
        public async Task AtCommand(
            [Summary("year")] int? year = null,
            [Summary("month")] int? month = null,
            [Summary("day")] int? day = null,
            [Summary("hour")] int? hour = null,
            [Summary("minute")] int? minute = null,
            [Summary("second")] int? second = null,
            [Summary("about")] string? message = null
        )
        {
            if (year is null && month is null && day is null && hour is null && minute is null && second is null)
            {
                await RespondWithEmbedAsync("Please specify a time.", type: ResponseType.Error);

                return;
            }

            await DeferAsync();

            var timeZoneId = (await db.GetUserAsync(Context.User.Id)).TimeZone;

            if (timeZoneId is null)
            {
                await FollowupWithEmbedAsync(
                    message: "To use this command, please set your time zone with `/settimezone` first.",
                    type: ResponseType.Error
                );

                return;
            }

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            try
            {
                var timestamp = GetTimestamp(year, month, day, hour, minute, second, Context.Interaction.CreatedAt,
                    timeZone);

                if (timestamp is null)
                {
                    await FollowupWithEmbedAsync(
                        message: "The specified time must be at least 10 seconds in the future.",
                        type: ResponseType.Error
                    );

                    return;
                }

                await reminderService.AddReminderAsync(new Reminder(Context.User.Id, Context.Channel.Id,
                    timestamp.Value, message));

                await FollowupAsync(embed:
                    new EmbedBuilder()
                        .WithColor(Color.Green)
                        .WithTitle("Reminder Set")
                        .WithDescription(
                            $"{message}\n".OnlyIf(message is not null)
                            + $"{timestamp.Value.ToDiscordTimestampDateTime()} ({timestamp.Value.ToDiscordTimestampRel()})"
                        )
                        .Build()
                );
            }
            catch (ArgumentOutOfRangeException)
            {
                await FollowupWithEmbedAsync("The specified time is invalid.", type: ResponseType.Error);
            }
        }

        private static DateTimeOffset? GetTimestamp(int? year, int? month, int? day, int? hour, int? minute, int? second, DateTimeOffset now, TimeZoneInfo tz)
        {
            if (!CorrectParameters(ref year, ref month, ref day, ref hour, ref minute, second, now, tz))
            {
                return null;
            }

            var userNow = TimeZoneInfo.ConvertTime(now, tz);

            var timestamp = new DateTimeOffset(
                year ?? 0,
                month ?? 1,
                day ?? 1,
                hour ?? 0,
                minute ?? 0,
                second ?? 0,
                tz.GetUtcOffset(userNow)
            );

            if (timestamp < now.AddSeconds(10))
            {
                return null;
            }

            return timestamp;
        }

        // I hate this but what am I supposed to do?
        private static bool CorrectParameters(ref int? year, ref int? month, ref int? day, ref int? hour, ref int? minute, int? second, DateTimeOffset now, TimeZoneInfo tz)
        {
            var userNow = TimeZoneInfo.ConvertTime(now, tz);

            if (year is null)
            {
                year ??= userNow.Year;

                if (month is null)
                {
                    month ??= userNow.Month;

                    if (day is null)
                    {
                        day ??= userNow.Day;

                        if (hour is null)
                        {
                            hour ??= userNow.Hour;

                            if (minute is null)
                            {
                                minute ??= userNow.Minute;

                                if (userNow.Second > second)
                                {
                                    minute++;
                                }
                            }

                            if (userNow.Minute > minute)
                            {
                                hour++;
                            }
                        }

                        if (userNow.Hour > hour)
                        {
                            day++;
                        }
                    }

                    if (userNow.Day > day)
                    {
                        month++;
                    }
                }

                if (userNow.Month > month)
                {
                    year++;
                }
            }
            else if (userNow.Year > year)
            {
                return false;
            }

            return true;
        }
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
                $"{reminder.Message}\n".OnlyIf(reminder.Message is not null)
                + $"{reminder.Timestamp.ToDiscordTimestampLong()} ({reminder.Timestamp.ToDiscordTimestampRel()})"
            );
        }

        await FollowupAsync(embed: embed.Build());
    }

    [UsedImplicitly]
    [SlashCommand("delete", "Removes a reminder.")]
    public async Task DeleteCommand(
        [Summary("index", "The index of the reminder to remove.")]
        int index
    )
    {
        await DeferAsync();

        var reminders = await GetOrderedRemindersByUserId();

        if (index < 1 || index > reminders.Count)
        {
            await FollowupWithEmbedAsync("There is no reminder with that index.", type: ResponseType.Error);

            return;
        }

        var reminder = reminders[index - 1];

        await reminderService.RemoveReminderAsync(reminder);

        await FollowupAsync(embed:
            new EmbedBuilder()
                .WithColor(Color.Red)
                .WithTitle("Reminder Removed")
                .WithDescription(
                    $"{reminder.Message}\n".OnlyIf(reminder.Message is not null)
                    + $"{reminder.Timestamp.ToDiscordTimestampLong()} ({reminder.Timestamp.ToDiscordTimestampRel()})"
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
