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

        await using var uow = uowFactory.Create();

        var autoPingOptions = await uow.AutoPingMappings.GetByChannelIdAsync(thread.ParentChannel.Id);

        if (autoPingOptions.Any(o => o.TagId is null)
            || autoPingOptions.Any(o => thread.AppliedTags.Any(t => t == o.TagId)))
        {
            var msg = string.Join(' ', autoPingOptions.Select(o => $"<@&{o.RoleId}>"));
            await thread.SendMessageAsync(msg);
        }
    }
}
