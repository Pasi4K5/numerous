using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using MongoDB.Driver;
using Numerous.Database;
using ScottPlot;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace Numerous.Discord.Commands;

[UsedImplicitly]
[Group("serverstats", "Shows stats about the server.")]
public sealed class ServerStatsCommandModule(DbManager db) : CommandModule
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

        var titlePostfix = startTime is null ? "" : $" since {startTime.Value:yyyy-MM-dd HH:mm:ss}";

        var channelNames = Context.Guild.Channels
            .Where(c => c is ITextChannel and not IThreadChannel)
            .OrderByDescending(x => x.Name)
            .ToDictionary(x => x.Id, x => x.Name);

        var messages = await db.DiscordMessages
            .FindAsync(m =>
                m.GuildId == Context.Guild.Id
                && (startTime == null || m.Timestamps.First() >= startTime)
                && channelNames.Keys.Any(x => x == m.ChannelId));

        var plt = new Plot(1200);
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
                        .FindAsync(m =>
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
