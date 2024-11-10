// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Util;
using Numerous.Database.Context;
using Numerous.Database.Dtos;

namespace Numerous.Bot.Discord.Interactions.Commands;

[UsedImplicitly]
[Group("reminder", "Reminds you.")]
public sealed class ReminderCommandModule(ReminderService reminderService, IUnitOfWork uow) : InteractionModule
{
    [UsedImplicitly]
    [Group("set", "Sets a reminder.")]
    public sealed class Set(ReminderService reminderService, IUnitOfWork uow) : InteractionModule
    {
        [UsedImplicitly]
        [SlashCommand("in", "Sets a reminder after the specified time.")]
        public async Task InCommand(
            [Summary("hours")] int? hours = null,
            [Summary("minutes")] int? minutes = null,
            [Summary("seconds")] int? seconds = null,
            [Summary("about")] string? message = null,
            [Summary("private")] bool priv = false
        )
        {
            var ephemeral = priv;
            priv |= Context.Channel is IDMChannel;

            if (hours is null && minutes is null && seconds is null)
            {
                await RespondWithEmbedAsync("Please specify a time.", type: ResponseType.Error);

                return;
            }

            var timestamp = Context.Interaction.CreatedAt;

            timestamp = timestamp
                .AddHours(hours ?? 0)
                .AddMinutes(minutes ?? 0)
                .AddSeconds(seconds ?? 0);

            if (timestamp < Context.Interaction.CreatedAt.AddSeconds(10))
            {
                await RespondWithEmbedAsync(message: "The specified time must be at least 10 seconds in the future.", type: ResponseType.Error);

                return;
            }

            await DeferAsync(ephemeral);

            await reminderService.AddReminderAsync(new ReminderDto
            {
                UserId = Context.User.Id,
                GuildId = Context.Guild?.Id,
                ChannelId = priv ? null : Context.Channel.Id,
                Timestamp = timestamp,
                Message = message,
            });

            await FollowupAsync(embed:
                new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithTitle("Reminder Set")
                    .WithDescription(
                        $"{message}\n".OnlyIf(message is not null)
                        + $"{timestamp.ToDiscordTimestampLong()} ({timestamp.ToDiscordTimestampRel()})"
                    )
                    .Build(),
                ephemeral: ephemeral
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
            [Summary("about")] string? message = null,
            [Summary("private")] bool priv = false
        )
        {
            var ephemeral = priv;
            priv |= Context.Channel is IDMChannel;

            if (year is null && month is null && day is null && hour is null && minute is null && second is null)
            {
                await RespondWithEmbedAsync("Please specify a time.", type: ResponseType.Error);

                return;
            }

            await DeferAsync(ephemeral);

            var timeZone = (await uow.DiscordUsers.GetAsync(Context.User.Id)).TimeZone;

            if (timeZone is null)
            {
                await FollowupWithEmbedAsync(
                    message: "To use this command, please set your time zone with `/settimezone` first.",
                    type: ResponseType.Error
                );

                return;
            }

            try
            {
                var now = Context.Interaction.CreatedAt;

                var timestamp = DateTimeUtil.ParametersToDateTime(
                    now.ToDateTime(timeZone),
                    (ushort?)year,
                    (byte?)month,
                    (byte?)day,
                    (byte?)hour,
                    (byte?)minute,
                    (byte?)second
                ).ToOffset(timeZone);

                if (timestamp < now.AddSeconds(10))
                {
                    throw new ArgumentOutOfRangeException();
                }

                await reminderService.AddReminderAsync(new ReminderDto
                {
                    UserId = Context.User.Id,
                    GuildId = Context.Guild?.Id,
                    ChannelId = priv ? null : Context.Channel.Id,
                    Timestamp = timestamp,
                    Message = message,
                });

                await FollowupAsync(embed:
                    new EmbedBuilder()
                        .WithColor(Color.Green)
                        .WithTitle("Reminder Set")
                        .WithDescription(
                            $"{message}\n".OnlyIf(message is not null)
                            + $"{timestamp.ToDiscordTimestampDateTime()} ({timestamp.ToDiscordTimestampRel()})"
                        )
                        .Build(),
                    ephemeral: ephemeral
                );
            }
            catch (InvalidCastException)
            {
                await FollowupWithEmbedAsync(
                    message: "The specified time is invalid.",
                    type: ResponseType.Error
                );
            }
            catch (ArgumentOutOfRangeException)
            {
                await FollowupWithEmbedAsync(
                    message: "The specified time must be at least 10 seconds in the future.",
                    type: ResponseType.Error
                );
            }
            catch (ArgumentNullException)
            {
                await FollowupWithEmbedAsync(
                    message: "Please specify at least on parameter.",
                    type: ResponseType.Error
                );
            }
        }
    }

    [UsedImplicitly]
    [SlashCommand("list", "Lists your reminders.")]
    public async Task ListCommand([Summary("private")] bool ephemeral = false)
    {
        await DeferAsync(ephemeral);

        var reminders = await uow.Reminders.GetOrderedRemindersAsync(Context.User.Id);

        var embed = new EmbedBuilder()
            .WithTitle("Your Reminders")
            .WithColor(Color.Blue);

        if (reminders.Length <= 0)
        {
            await FollowupAsync(embed: embed.WithDescription("You don't have any reminders.").Build());

            return;
        }

        foreach (var (reminder, index) in reminders.WithIndexes())
        {
            embed.AddField(
                $"Reminder {index + 1} in <#{reminder.ChannelId ?? (await Context.User.CreateDMChannelAsync()).Id}>",
                (reminder.ChannelId is not null || Context.Channel is IDMChannel || ephemeral
                    ? $"{reminder.Message}\n".OnlyIf(reminder.Message is not null)
                    : "*[REDACTED]*\n"
                )
                + $"{reminder.Timestamp.ToDiscordTimestampLong()} ({reminder.Timestamp.ToDiscordTimestampRel()})"
            );
        }

        await FollowupAsync(embed: embed.Build(), ephemeral: ephemeral);
    }

    [UsedImplicitly]
    [SlashCommand("delete", "Removes a reminder.")]
    public async Task DeleteCommand(
        [Summary("index", "The index of the reminder to remove.")]
        int index
    )
    {
        await DeferAsync();

        var reminders = await uow.Reminders.GetOrderedRemindersAsync(Context.User.Id);

        if (index < 1 || index > reminders.Length)
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
}
