// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Numerous.DependencyInjection;

namespace Numerous.Discord.Events;

[HostedService]
public class MudaeMessageHandler(DiscordSocketClient client) : IHostedService
{
    private readonly TimeSpan _claimTimeSpan = TimeSpan.FromSeconds(45);

    private Dictionary<ulong, DateTimeOffset> _firstClaimTimeouts = new();
    private Dictionary<ulong, DateTimeOffset> _lastClaimTimeouts = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.MessageReceived += HandleMessageReceived;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task HandleMessageReceived(IMessage msg)
    {
        if (
            msg.Author.Id != Constants.MudaeUserId
            || !msg.Embeds.Any(e => e.Description.EndsWith("React with any emoji to claim!"))
        )
        {
            return;
        }

        var channelId = msg.Channel.Id;
        var currentTimeout = msg.Timestamp + _claimTimeSpan;

        _firstClaimTimeouts.TryAdd(channelId, currentTimeout);
        _lastClaimTimeouts.TryAdd(channelId, currentTimeout);

        if (_lastClaimTimeouts[channelId] < msg.Timestamp)
        {
            _firstClaimTimeouts[channelId] = currentTimeout;
        }

        _lastClaimTimeouts[channelId] = currentTimeout;

        await msg.ReplyAsync(
            $"Claim timeout: {currentTimeout.ToDiscordTimestampRel()}"
            + (
                currentTimeout != _firstClaimTimeouts[channelId]
                    ? $"\n**First roll timeout: {_firstClaimTimeouts[channelId].ToDiscordTimestampRel()}**"
                    : ""
            )
        );
    }
}
