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
using Numerous.Common;
using Numerous.Common.Util;
using Numerous.Database.Context;

namespace Numerous.Bot.Osu;

public sealed class MapfeedService(
    IHost host,
    DiscordSocketClient discordClient,
    IOsuApiRepository api,
    IUnitOfWorkFactory uowFactory
) : HostedService
{
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
    private static readonly DateTimeOffset _startupTime = DateTime.Now;

    private readonly Dictionary<uint, DateTimeOffset> _sentSets = new();

    public override Task StartAsync(CancellationToken ct)
    {
        host.Services.UseScheduler(s => s.ScheduleAsync(() => CheckForNewBeatmapsetsAsync(ct)).EveryTenSeconds().PreventOverlapping(nameof(MapfeedService)));

        return Task.CompletedTask;
    }

    private async Task CheckForNewBeatmapsetsAsync(CancellationToken ct)
    {
        await using var uow = uowFactory.Create();

        var channelIds = await uow.MessageChannels.GetAllMapfeedChannelIdsAsync(ct);
        var osuUsers = await uow.OsuUsers.GetVerifiedWithDiscordId(ct);

        if (channelIds.Length == 0 || osuUsers.Count == 0)
        {
            return;
        }

        var beatmapsets = (await api.SearchRecentlyChangedBeatmapsetsAsync())
            .Where(s =>
                (s.RankedDate ?? s.SubmittedDate) >= DtMath.Max(_startupTime, DateTimeOffset.UtcNow - _cacheDuration)
                && !_sentSets.ContainsKey(s.Id)
            ).OrderBy(s => s.RankedDate ?? s.SubmittedDate);

        foreach (var set in beatmapsets)
        {
            var allMapperOsuIds = set.RelatedUsers
                .Select(u => u.Id)
                .Prepend(set.UserId)
                .ToArray();

            if (allMapperOsuIds.All(id => !osuUsers.ContainsKey(id)))
            {
                // No mapper or GDer verified.
                continue;
            }

            var allMapperDiscordIds = allMapperOsuIds
                .Where(osuUsers.ContainsKey)
                .Select(osuId => osuUsers[osuId])
                .ToArray();

            var mapper = osuUsers.TryGetValue(set.UserId, out var userId)
                ? MentionUtils.MentionUser(userId)
                : Link.OsuUser(set.UserId, set.Creator);

            var verifiedGdMapperDiscordIds = set.RelatedUsers
                .Where(u => osuUsers.ContainsKey(u.Id))
                .Select(u => osuUsers[u.Id]);

            var sendTasks = channelIds.Select(id => Task.Run(async () =>
            {
                var channel = (IMessageChannel)await discordClient.GetChannelAsync(id);
                var guild = ((IGuildChannel)channel).Guild;
                await guild.DownloadUsersAsync();
                var guildUserIds = (await guild.GetUsersAsync()).Select(u => u.Id);

                if (allMapperDiscordIds.All(discordId => guildUserIds.Contains(discordId)))
                {
                    // No mapper or GDer in guild.
                    return;
                }

                var gdMappersInGuild = verifiedGdMapperDiscordIds
                    .Where(discordId => guildUserIds.Contains(discordId))
                    .Select(MentionUtils.MentionUser)
                    .ToArray();

                var (eb, cb) = EmbedBuilders.BeatmapSetUpdate(set, mapper, gdMappersInGuild);
                await channel.SendMessageAsync(embed: eb.Build(), components: cb.Build());
            }, ct));

            await Task.WhenAll(sendTasks);

            _sentSets[set.Id] = set.RankedDate ?? set.SubmittedDate;
        }

        foreach (var (id, submittedDate) in _sentSets)
        {
            if (submittedDate < DateTimeOffset.Now - _cacheDuration)
            {
                _sentSets.Remove(id);
            }
        }
    }
}
