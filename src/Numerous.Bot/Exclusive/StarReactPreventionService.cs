// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;
using Numerous.Common.Config;
using Numerous.Common.Services;

namespace Numerous.Bot.Exclusive;

public sealed class StarReactPreventionService(DiscordSocketClient client, IConfigProvider cfgProv) : HostedService
{
    public override Task StartAsync(CancellationToken ct)
    {
        client.ReactionAdded += async (cacheableMsg, cacheableChannel, reaction) =>
        {
            var emoji = new Emoji("⭐");

            if (!Equals(reaction.Emote, emoji))
            {
                return;
            }

            var channel = cacheableChannel.HasValue
                ? cacheableChannel.Value
                : (IMessageChannel)await client.GetChannelAsync(cacheableChannel.Id);

            if ((channel as IGuildChannel)?.GuildId != cfgProv.Get().ExclusiveServerId)
            {
                return;
            }

            var msg = cacheableMsg.HasValue
                ? cacheableMsg.Value
                : await channel.GetMessageAsync(cacheableMsg.Id);

            await foreach (var page in msg.GetReactionUsersAsync(emoji, 1000).WithCancellation(ct))
            {
                foreach (var user in page)
                {
                    if (user.Id == msg.Author.Id)
                    {
                        await msg.RemoveReactionAsync(emoji, user);

                        return;
                    }
                }
            }
        };

        return base.StartAsync(ct);
    }
}
