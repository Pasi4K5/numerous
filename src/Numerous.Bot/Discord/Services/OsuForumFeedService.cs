// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Numerous.Bot.Discord.Util;
using Numerous.Bot.Web.Osu;
using Numerous.Bot.Web.Osu.Models;
using Numerous.Common.Services;
using Numerous.Common.Util;
using Numerous.Database.Context;

namespace Numerous.Bot.Discord.Services;

public sealed class OsuForumFeedService(
    IHost host,
    IUnitOfWorkFactory uowFactory,
    IOsuApiRepository osuApi,
    DiscordSocketClient discordClient,
    EmbedBuilders eb
) : HostedService
{
    private static readonly int[] _ignoreRepliesForumIds =
    [
        53, 78, 115,
    ];

    private DateTimeOffset _lastChecked = DateTimeOffset.UtcNow;

    public override Task StartAsync(CancellationToken ct)
    {
        host.Services.UseScheduler(s => s.ScheduleAsync(() =>
            RunAsync(ct)
        ).EveryTenSeconds().PreventOverlapping(nameof(OsuForumFeedService)));

        return Task.CompletedTask;
    }

    private async Task RunAsync(CancellationToken ct = default)
    {
        await using var uow = uowFactory.Create();

        var (subscriptions, topicMetas) = (
            await uow.MessageChannels.GetForumSubscriptionsAsync(ct),
            await osuApi.GetForumTopicsAsync(ct: ct)
        );

        foreach (var topic in topicMetas.Where(t => _ignoreRepliesForumIds.Contains(t.ForumId)))
        {
            // Treat them as if they were not updated
            topic.UpdatedAt = topic.CreatedAt;
        }

        var updatedTopicsMeta = topicMetas
            .Where(t => t.UpdatedAt > _lastChecked && subscriptions.ContainsKey(t.ForumId))
            .ToArray();

        if (updatedTopicsMeta.Length == 0)
        {
            return;
        }

        var updatedTopics = await updatedTopicsMeta.Select(t =>
            osuApi.GetForumTopicAsync(t.Id, IOsuApi.ForumPostSort.Desc, ct)
        );

        List<(ApiForumPost post, ApiForumTopicMeta meta)> newPosts = new();

        foreach (var topic in updatedTopics)
        {
            var posts = topic.Posts
                .Where(p => p.CreatedAt > _lastChecked)
                .Select(p => (p, topic.Meta))
                .ToList();

            if (posts.Count > 0)
            {
                newPosts.AddRange(posts);
            }
        }

        _lastChecked = topicMetas.Max(t => t.UpdatedAt);

        var channels = (await subscriptions.Values
                .SelectMany(ids => ids)
                .Distinct()
                .Select(id => discordClient.GetChannelAsync(id))
            ).Select(ch => (IMessageChannel)ch)
            .ToArray();

        var postEmbeds = (await newPosts.Select(async x => (x.post, embedBuilder: await eb.ForumPostAsync(x.meta, x.post))))
            .ToDictionary(x => x.post, x => x.embedBuilder.Build());

        foreach (var (post, embed) in postEmbeds.OrderBy(e => e.Key.CreatedAt))
        {
            await channels
                .Where(c => subscriptions.TryGetValue(post.ForumId, out var ids) && ids.Contains(c.Id))
                .Select(c => c.SendMessageAsync(embed: embed));
        }
    }
}
