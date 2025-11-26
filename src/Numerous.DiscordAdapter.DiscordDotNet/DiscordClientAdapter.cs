// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Frozen;
using Discord.WebSocket;
using Numerous.DiscordAdapter.Channels;
using Numerous.DiscordAdapter.DiscordDotNet.Channels;
using Numerous.DiscordAdapter.DiscordDotNet.Guilds;
using Numerous.DiscordAdapter.DiscordDotNet.Users;
using Numerous.DiscordAdapter.Guilds;
using Numerous.DiscordAdapter.Users;

namespace Numerous.DiscordAdapter.DiscordDotNet;

internal sealed class DiscordClientAdapter : IDiscordClient
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordChannelAdapterFactory _channelFactory;

    public IReadOnlyCollection<IDiscordGuild> Guilds =>
        _client.Guilds.Select(IDiscordGuild (g) => new DiscordGuildAdapter(g)).ToFrozenSet();

    public event Func<IDiscordGuildMember, Task>? GuildMemberAdd;
    public event Func<IDiscordGuildMember, Task>? GuildMemberUpdated;

    public DiscordClientAdapter
    (
        DiscordSocketClient client,
        DiscordChannelAdapterFactory channelFactory
    )
    {
        _client = client;
        _channelFactory = channelFactory;

        _client.UserJoined += async user =>
        {
            if (GuildMemberAdd is not null)
            {
                await GuildMemberAdd(new DiscordGuildMemberAdapter(user));
            }
        };

        _client.GuildMemberUpdated += async (_, after) =>
        {
            if (GuildMemberUpdated is not null)
            {
                await GuildMemberUpdated(new DiscordGuildMemberAdapter(after));
            }
        };
    }

    public async Task<IDiscordChannel> GetChannelAsync(ulong id) =>
        _channelFactory.Wrap(await _client.GetChannelAsync(id));

    public async IAsyncEnumerable<IDiscordGuildMember> GetGuildMembersAsync(ulong guildId)
    {
        var guild = _client.GetGuild(guildId);
        await guild.DownloadUsersAsync();

        foreach (var user in guild.Users)
        {
            yield return new DiscordGuildMemberAdapter(user);
        }
    }

    public async Task<IDiscordGuildMember?> GetGuildMemberAsync(ulong guildId, ulong userId)
    {
        var member = await _client.Rest.GetGuildUserAsync(guildId, userId);

        return member is not null ? new DiscordGuildMemberAdapter(member) : null;
    }
}
