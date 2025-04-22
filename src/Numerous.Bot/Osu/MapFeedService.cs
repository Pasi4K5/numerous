// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Numerous.Bot.Discord.Util;
using Numerous.Bot.Util;
using Numerous.Bot.Web.Osu;
using Numerous.Common.Services;
using Numerous.Common.Util;
using Numerous.Database.Context;
using osu.Game.Beatmaps;

namespace Numerous.Bot.Osu;

public sealed class MapFeedService(
    IHost host,
    DiscordSocketClient discordClient,
    IOsuApiRepository api,
    IUnitOfWorkFactory uowFactory
) : HostedService
{
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
    private static readonly DateTimeOffset _startupTime = DateTime.Now;

    private readonly Dictionary<int, DateTimeOffset> _checkedSets = new();

    public override Task StartAsync(CancellationToken ct)
    {
        host.Services.UseScheduler(s => s.ScheduleAsync(() => CheckForNewBeatmapsetsAsync(ct)).EveryTenSeconds().PreventOverlapping(nameof(MapFeedService)));

        return Task.CompletedTask;
    }

    private async Task CheckForNewBeatmapsetsAsync(CancellationToken ct)
    {
        await using var uow = uowFactory.Create();

        var channelIds = await uow.MessageChannels.GetAllMapfeedChannelIdsAsync(ct);
        var osuUserDiscordIdDic = await uow.OsuUsers.GetVerifiedWithDiscordIdAsync(ct);

        if (channelIds.Length == 0 || osuUserDiscordIdDic.Count == 0)
        {
            return;
        }

        var beatmapSets = (await api.SearchRecentlyChangedBeatmapsetsAsync(ct))
            .Where(s =>
                (s.RankedDate ?? s.SubmittedDate) >= DtMath.Max(_startupTime, DateTimeOffset.UtcNow - _cacheDuration)
                && !_checkedSets.ContainsKey(s.Id)
            ).OrderBy(s => s.RankedDate ?? s.SubmittedDate)
            .ToArray();

        foreach (var set in beatmapSets)
        {
            // Pending/WIP maps can't have GDs yet, because they have just been submitted.
            var canHaveGds = set.Ranked is not (BeatmapOnlineStatus.WIP or BeatmapOnlineStatus.Pending);

            if (!canHaveGds && !osuUserDiscordIdDic.ContainsKey(set.UserId))
            {
                // Mapper not verified.
                continue;
            }

            var fullSet = await api.GetBeatmapsetAsync(set.Id, ct);

            var gdMapperIds = fullSet.Beatmaps
                .SelectMany(b => b.Owners!)
                .Where(o => o.Id != fullSet.UserId)
                .Select(o => o.Id)
                .ToHashSet();

            var allMapperOsuIds = gdMapperIds
                .Append(fullSet.UserId)
                .ToHashSet();

            if (allMapperOsuIds.All(id => !osuUserDiscordIdDic.ContainsKey(id)))
            {
                // No mapper or GDer verified.
                continue;
            }

            var allMapperDiscordIds = allMapperOsuIds
                .Where(osuUserDiscordIdDic.ContainsKey)
                .Select(osuId => osuUserDiscordIdDic[osuId])
                .ToHashSet();

            var mapper = osuUserDiscordIdDic.TryGetValue(fullSet.UserId, out var userId)
                ? $"{MentionUtils.MentionUser(userId)} ({Link.OsuUser(fullSet.UserId, fullSet.Creator)})"
                : Link.OsuUser(fullSet.UserId, fullSet.Creator);

            var verifiedGdMapperIds = gdMapperIds
                .Where(id => osuUserDiscordIdDic.ContainsKey(id))
                .Select(id => new { osuId = id, discordId = osuUserDiscordIdDic[id] });

            var usernames = fullSet.RelatedUsers?.ToDictionary(u => u.Id, u => u.Username)
                            ?? [];

            var sendTasks = channelIds.Select(id => Task.Run(async () =>
            {
                var channel = (IMessageChannel)await discordClient.GetChannelAsync(id);
                var guild = ((IGuildChannel)channel).Guild;
                await guild.DownloadUsersAsync();
                var guildUserIds = (await guild.GetUsersAsync()).Select(u => u.Id);

                if (allMapperDiscordIds.All(discordId => !guildUserIds.Contains(discordId)))
                {
                    // No mapper or GDer in guild.
                    return;
                }

                var gdMappersInGuild = verifiedGdMapperIds
                    .Where(ids => guildUserIds.Contains(ids.discordId))
                    .Select(ids => $"{MentionUtils.MentionUser(ids.discordId)} ({Link.OsuUser(ids.osuId, usernames[ids.osuId])})")
                    .ToArray();

                var (eb, cb) = EmbedBuilders.BeatmapSetUpdate(fullSet, mapper, gdMappersInGuild);
                await channel.SendMessageAsync(embed: eb.Build(), components: cb.Build());
            }, ct));

            await Task.WhenAll(sendTasks);
        }

        foreach (var set in beatmapSets)
        {
            _checkedSets[set.Id] = set.RankedDate ?? set.SubmittedDate;
        }

        foreach (var (id, submittedDate) in _checkedSets)
        {
            if (submittedDate < DateTimeOffset.Now - _cacheDuration)
            {
                _checkedSets.Remove(id);
            }
        }
    }
}
