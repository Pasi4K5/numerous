// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using Numerous.Common.Services;
using Numerous.DiscordAdapter;
using Numerous.DiscordAdapter.Users;

namespace Numerous.Bot.Discord.Services;

public sealed class DiscordUserCachingService
    : HostedService
{
    public delegate Task GuildUserUpdatedHandler(IDiscordGuildMember before, IDiscordGuildMember after);

    public event GuildUserUpdatedHandler? GuildUserUpdated;

    private readonly IDiscordClient _client;
    private readonly ConcurrentDictionary<ulong, IDiscordGuildMember> _userCache = new();

    public DiscordUserCachingService(IDiscordClient client)
    {
        _client = client;

        _client.GuildMemberUpdated += user =>
        {
            if (_userCache.TryGetValue(user.Id, out var before))
            {
                _userCache[user.Id] = user;

                return GuildUserUpdated?.Invoke(before, user) ?? Task.CompletedTask;
            }

            _userCache[user.Id] = user;

            return Task.CompletedTask;
        };
    }

    public override async Task StartAsync(CancellationToken ct) =>
        await Parallel.ForEachAsync(_client.Guilds.Select(g => g.Id), ct, async (guildId, _) =>
        {
            await foreach (var user in _client.GetGuildMembersAsync(guildId).WithCancellation(ct))
            {
                _userCache[user.Id] = user;
            }
        });
}
