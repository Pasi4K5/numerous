// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;
using Numerous.Bot.Util;

namespace Numerous.Bot.Discord.Events;

public partial class DiscordEventHandler
{
    // This is a workaround because the ThreadCreated event is always raised twice for some reason.
    private readonly List<ulong> _alreadyPinged = new();

    [Init]
    private void AutoPing_Init()
    {
        client.ThreadCreated += async u => await AutoPing(u);
    }

    private async Task AutoPing(SocketThreadChannel thread)
    {
        if (thread.ParentChannel is not IForumChannel)
        {
            return;
        }

        if (_alreadyPinged.Contains(thread.Id))
        {
            _alreadyPinged.Remove(thread.Id);

            return;
        }

        _alreadyPinged.Add(thread.Id);

        var options = await db.GuildOptions.FindOrInsertByIdAsync(thread.Guild.Id);
        var autoPingOptions = options.AutoPingOptions
            .Where(x => x.ChannelId == thread.ParentChannel.Id)
            .ToArray();

        if (autoPingOptions.Any(o => o.Tag is null)
            || autoPingOptions.Any(o => thread.AppliedTags.Any(t => t == o.Tag)))
        {
            var msg = string.Join(' ', autoPingOptions.Select(o => $"<@&{o.RoleId}>"));
            await thread.SendMessageAsync(msg);
        }
    }
}
