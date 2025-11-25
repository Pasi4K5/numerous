// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.WebSocket;
using Numerous.Database.Context;

namespace Numerous.Bot.Discord.Services;

public sealed class GuildStatsService
(
    DiscordSocketClient client,
    IUnitOfWorkFactory uowFactory
)
{
    public void Start()
    {
        client.UserJoined += async gu => await UpdateStatsAsync(gu.Guild);
        client.UserLeft += async (g, _) => await UpdateStatsAsync(g);
    }

    private async Task UpdateStatsAsync(SocketGuild guild)
    {
        var now = DateTimeOffset.UtcNow;
        await guild.DownloadUsersAsync();

        await using var uow = uowFactory.Create();

        await uow.GuildStats.InsertAsync(new()
        {
            GuildId = guild.Id,
            Timestamp = now,
            MemberCount = guild.DownloadedMemberCount,
        });

        await uow.CommitAsync();
    }
}
