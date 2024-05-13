// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using MongoDB.Driver;
using Numerous.Bot.Database;
using Numerous.Bot.Util;
using ScottPlot;
using Color = System.Drawing.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Numerous.Bot.Discord.Commands;

[UsedImplicitly]
[Group("serverstats", "Shows stats about the server.")]
public sealed class ServerStatsCommandModule(IDbService db) : InteractionModule
{
    [UsedImplicitly]
    [SlashCommand("channels", "Shows a chart of the given variable for each channel in the server.")]
    public async Task ChannelsCommand(
        [Summary("type", "The type of chart to show.")]
        [Choice("Pie", (int)ChartType.Pie)]
        [Choice("Line", (int)ChartType.Line)]
        ChartType type,
        [Summary("variable", "The variable that should be plotted.")]
        [Choice("Message count", (int)Variable.MessagesCount)]
        Variable variable,
        [Summary("timeframe", "The timeframe to show stats for.")]
        uint? timeframe = null,
        [Summary("timeunit", "The unit of time to use for the timeframe.")]
        [Choice("Hours", (int)TimeUnit.Hours)]
        [Choice("Days", (int)TimeUnit.Days)]
        [Choice("Weeks", (int)TimeUnit.Weeks)]
        [Choice("Months", (int)TimeUnit.Months)]
        TimeUnit? timeUnit = null
    )
    {
        if (timeframe is null ^ timeUnit is null)
        {
            await RespondAsync("You must specify both a timeframe and a time unit.");

            return;
        }

        await DeferAsync();

        DateTime? startTime = timeframe is null
            ? null
            : DateTime.UtcNow - GetTimeSpan(timeframe.Value, timeUnit!.Value);

        var titlePostfix = $" since {startTime:yyyy-MM-dd HH:mm:ss}".OnlyIf(startTime is not null);

        var channelNames = Context.Guild.Channels
            .Where(c => c is ITextChannel and not IThreadChannel)
            .OrderByDescending(x => x.Name)
            .ToDictionary(x => x.Id, x => x.Name);

        var messages = await db.DiscordMessages
            .FindManyAsync(m =>
                m.GuildId == Context.Guild.Id
                && (startTime == null || m.Timestamps.First() >= startTime)
                && channelNames.Keys.Any(x => x == m.ChannelId));

        var plt = new Plot(1280, 720);
        plt.Style(
            Color.FromArgb(0x313338).Opaque(),
            Color.FromArgb(0x2b2d31).Opaque(),
            Color.FromArgb(0x7a7e88).Opaque(),
            Color.FromArgb(0x7a7e88).Opaque(),
            Color.White,
            Color.White
        );
        var legend = plt.Legend();

        switch (type)
        {
            case ChartType.Pie:
            {
                var channelMessageCounts = (await messages.ToListAsync())
                    .GroupBy(m => m.ChannelId)
                    .Select(g => (g.Key, g.Count()))
                    .OrderByDescending(x => x.Item2)
                    .ToArray();

                var pie = plt.AddPie(channelMessageCounts.Select(x => (double)x.Item2).ToArray());
                pie.SliceLabels = channelMessageCounts.Select(x => "#" + (Context.Guild.GetChannel(x.Item1)?.Name ?? x.Item1.ToString())).ToArray();
                plt.Title("Distribution of messages per channel" + titlePostfix);

                break;
            }
            case ChartType.Line:
            {
                plt.XAxis.DateTimeFormat(true);
                plt.YAxis.Label("Number of messages");
                plt.Title("Cumulative number of messages per channel over time" + titlePostfix);
                legend.Location = Alignment.UpperLeft;

                var channelMessageCounts = (await messages.ToListAsync())
                    .GroupBy(m => m.ChannelId)
                    .Select(g => (g.Key, g.Count()))
                    .OrderByDescending(x => x.Item2)
                    .ToArray();

                foreach (var (channelId, _) in channelMessageCounts)
                {
                    var channelMessages = await db.DiscordMessages
                        .FindManyAsync(m =>
                            m.GuildId == Context.Guild.Id
                            && m.ChannelId == channelId
                            && (startTime == null || m.Timestamps.First() >= startTime));

                    var timestamps = (await channelMessages.ToListAsync())
                        .SelectMany(m => m.Timestamps)
                        .OrderBy(x => x)
                        .ToArray();

                    var cumulativeMessageCounts = Enumerable.Range(1, timestamps.Length)
                        .Select(i => (double)i)
                        .ToArray();

                    var scatter = plt.AddScatter(
                        timestamps.Select(x => x.DateTime.ToOADate()).ToArray(),
                        cumulativeMessageCounts, label: "#" + channelNames[channelId]
                    );
                    scatter.MarkerShape = MarkerShape.none;
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        var stream = new MemoryStream();
        plt.GetBitmap().Save(stream, ImageFormat.Png);
        await FollowupWithFileAsync(stream, "chart.png");
    }

    private static TimeSpan GetTimeSpan(uint timeframe, TimeUnit timeUnit)
    {
        return timeUnit switch
        {
            TimeUnit.Hours => TimeSpan.FromHours(timeframe),
            TimeUnit.Days => TimeSpan.FromDays(timeframe),
            TimeUnit.Weeks => TimeSpan.FromDays(timeframe * 7),
            TimeUnit.Months => TimeSpan.FromDays(timeframe * 30),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit), timeUnit, null),
        };
    }

    public enum Variable
    {
        MessagesCount,
    }

    public enum ChartType
    {
        Pie,
        Line,
    }

    public enum TimeUnit
    {
        Hours,
        Days,
        Weeks,
        Months,
    }
}
