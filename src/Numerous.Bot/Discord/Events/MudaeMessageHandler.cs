// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;
using Numerous.Bot.Discord.Util;
using Numerous.Common;
using Numerous.Common.Services;

namespace Numerous.Bot.Discord.Events;

public sealed class MudaeMessageHandler(DiscordSocketClient client) : HostedService
{
    private readonly TimeSpan _timeBetweenRollGroups = TimeSpan.FromSeconds(10);

    private readonly Dictionary<ulong, string> _firstClaimMessageLinks = new();
    private readonly Dictionary<ulong, DateTimeOffset> _lastRollTimes = new();

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        client.MessageReceived += HandleMessageReceived;

        return Task.CompletedTask;
    }

    private async Task HandleMessageReceived(IMessage msg)
    {
        if (msg.Content.Trim().Equals("top", StringComparison.OrdinalIgnoreCase)
            && !msg.Author.IsBot
            && _firstClaimMessageLinks.TryGetValue(msg.Channel.Id, out var link))
        {
            await msg.ReplyAsync(link);

            return;
        }

        var isReactionRoll =
            msg.Embeds.Count == 1
            && msg.Embeds.First().Description.EndsWith("React with any emoji to claim!");
        var actionRowComponents = (msg.Components.FirstOrDefault() as ActionRowComponent)?.Components;
        var isButtonRoll =
            msg.Components.Count == 1
            && actionRowComponents?.Count == 1
            && actionRowComponents.First().Type == ComponentType.Button;

        if (msg.Author.Id != Constants.MudaeUserId || (!isReactionRoll && !isButtonRoll))
        {
            return;
        }

        var channelId = msg.Channel.Id;

        _firstClaimMessageLinks.TryAdd(channelId, msg.GetLink());
        _lastRollTimes.TryAdd(channelId, msg.Timestamp);

        if (msg.Timestamp > _lastRollTimes[channelId] + _timeBetweenRollGroups)
        {
            _firstClaimMessageLinks[channelId] = msg.GetLink();
        }

        _lastRollTimes[channelId] = msg.Timestamp;
    }
}
