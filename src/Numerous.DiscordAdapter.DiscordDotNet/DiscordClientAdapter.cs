// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.WebSocket;
using Numerous.DiscordAdapter.Channels;
using Numerous.DiscordAdapter.DiscordDotNet.Channels;
using Numerous.DiscordAdapter.DiscordDotNet.Users;
using Numerous.DiscordAdapter.Users;

namespace Numerous.DiscordAdapter.DiscordDotNet;

internal sealed class DiscordClientAdapter
(
    DiscordSocketClient client,
    DiscordChannelAdapterFactory channelFactory
)
    : IDiscordClientAdapter
{
    public async Task<IDiscordChannel> GetChannelAsync(ulong id) =>
        channelFactory.Wrap(await client.GetChannelAsync(id));

    public async IAsyncEnumerable<IDiscordGuildUser> GetGuildUsersAsync(ulong guildId)
    {
        var guild = client.GetGuild(guildId);
        await guild.DownloadUsersAsync();

        foreach (var user in guild.Users)
        {
            yield return new DiscordGuildUserAdapter(user);
        }
    }
}
