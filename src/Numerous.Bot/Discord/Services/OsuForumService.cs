// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Microsoft.Extensions.Hosting;
using Numerous.Bot.Discord.Util;
using Numerous.Bot.Web.Osu;
using Numerous.Bot.Web.Osu.Models;
using Numerous.Common.Services;
using Numerous.Common.Util;
using Numerous.Database.Context;
using Numerous.DiscordAdapter;
using Numerous.DiscordAdapter.Channels;

namespace Numerous.Bot.Discord.Services;

public sealed class OsuForumService
(
    IHost host,
    IUnitOfWorkFactory uowFactory,
    IOsuApiRepository osuApi,
    IDiscordClientAdapter discordClient,
    EmbedBuilders eb
) : HostedService
{
    private static readonly int[] _ignoreRepliesForumIds =
    [
        53, 87, 115,
    ];

    private DateTimeOffset _lastChecked = DateTimeOffset.UtcNow;

    public override Task StartAsync(CancellationToken ct)
    {
        host.Services.UseScheduler(s => s.ScheduleAsync(() =>
            RunAsync(ct)
        ).EveryFifteenSeconds().RunOnceAtStart().PreventOverlapping(nameof(OsuForumService)));

        return Task.CompletedTask;
    }

    private async Task RunAsync(CancellationToken ct = default)
    {
        await using var uow = uowFactory.Create();

        var topicMetas = await osuApi.GetForumTopicsAsync(ct: ct);

        if (topicMetas.Length == 0)
        {
            return;
        }

        var updatedTopicMetas = topicMetas
            .Where(t => t.UpdatedAt > _lastChecked)
            .ToArray();

        if (updatedTopicMetas.Length > 0)
        {
            await RunForumFeedAsync(updatedTopicMetas, ct);
            await RunTopicFeedAsync(updatedTopicMetas, ct);
        }

        _lastChecked = topicMetas.Max(t => t.UpdatedAt);
    }

    private async Task RunForumFeedAsync(ApiForumTopicMeta[] updatedTopicMetas, CancellationToken ct)
    {
        await using var uow = uowFactory.Create();

        var subscriptions = await uow.MessageChannels.GetForumSubscriptionsAsync(ct);

        var subscribedTopicMetas = updatedTopicMetas
            .Where(t =>
                subscriptions.ContainsKey(t.ForumId)
                && (_ignoreRepliesForumIds.Contains(t.ForumId) ? t.CreatedAt : t.UpdatedAt) > _lastChecked
            ).ToArray();

        if (subscribedTopicMetas.Length == 0)
        {
            return;
        }

        var subscribedTopics = await subscribedTopicMetas.Select(t =>
            osuApi.GetForumTopicAsync(t.Id, IOsuApi.ForumPostSort.Desc, ct)
        );

        var newPosts = subscribedTopics.SelectMany(topic =>
            topic.Posts
                .Where(p => p.CreatedAt > _lastChecked)
                .Select(p => (post: p, meta: topic.Meta))
                .ToArray()
        );

        var channels = (await subscriptions.Values
                .SelectMany(ids => ids)
                .Distinct()
                .Select(discordClient.GetChannelAsync)
            ).Select(ch => (IDiscordMessageChannel)ch)
            .ToArray();

        var postEmbeds = (await newPosts.Select(async x => (x.post, embedBuilder: await eb.ForumPostAsync(x.meta, x.post))))
            .ToDictionary(x => x.post, x => x.embedBuilder);

        foreach (var (post, embed) in postEmbeds.OrderBy(e => e.Key.CreatedAt))
        {
            await channels
                .Where(c => subscriptions.TryGetValue(post.ForumId, out var ids) && ids.Contains(c.Id))
                .Select(c => c.SendMessageAsync(new() { Embeds = [embed] }));
        }
    }

    private async Task RunTopicFeedAsync(ApiForumTopicMeta[] updatedTopicMetas, CancellationToken ct)
    {
        await using var uow = uowFactory.Create();
        var subscriptions = await uow.MessageChannels.GetTopicSubscriptionsAsync().ToListAsync(ct);

        var topicMetas = updatedTopicMetas
            .Where(t => subscriptions.Any(s => s.TopicId == t.Id))
            .ToArray();

        var topics = await topicMetas.Select(t =>
            osuApi.GetForumTopicAsync(t.Id, IOsuApi.ForumPostSort.Desc, ct)
        );

        var embeds = topics
            .ToDictionary(
                t => t.Meta.Id,
                t => t.Posts
                    .Where(p =>
                        p.CreatedAt > _lastChecked
                        // This feature is meant for modding queues,
                        // so filter out everything that isn't a mod request.
                        && p.Body.Raw.Contains("beatmaps", StringComparison.OrdinalIgnoreCase)
                    )
                    .Select(async p => (await eb.ForumPostAsync(t.Meta, p)))
            );

        foreach (var (topicId, channelId) in subscriptions)
        {
            if (!embeds.TryGetValue(topicId, out var topicEmbeds))
            {
                continue;
            }

            var channel = (IDiscordMessageChannel)await discordClient.GetChannelAsync(channelId);

            foreach (var embed in await topicEmbeds)
            {
                await channel.SendMessageAsync(new()
                {
                    Embeds = [embed],
                });
            }
        }
    }
}
