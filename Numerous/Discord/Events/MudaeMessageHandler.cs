// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Numerous.DependencyInjection;

namespace Numerous.Discord.Events;

[HostedService]
public class MudaeMessageHandler(DiscordSocketClient client) : IHostedService
{
    private const byte ClaimTimeout = 45;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.MessageReceived += HandleMessageReceived;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task HandleMessageReceived(SocketMessage msg)
    {
        if (
            msg.Author.Id != Constants.MudaeUserId
            || !msg.Embeds.Any(e => e.Description.EndsWith("React with any emoji to claim!"))
        )
        {
            return;
        }

        await msg.ReplyAsync(
            $"Claim timer expires {msg.Timestamp.AddSeconds(ClaimTimeout).ToDiscordTimestampRel()}"
        );
    }
}
