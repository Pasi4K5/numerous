// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;

namespace Numerous.Bot.Discord.Commands;

public sealed class WantButNoClaimCommandModule : CommandModule
{
    private const ulong PingTargetUserId = 345885199386804235;

    [UsedImplicitly]
    [MessageCommand("I want this but I have no claim")]
    public async Task WantButNoClaimCommand(IMessage msg)
    {
        if (msg.Author.Id != Constants.MudaeUserId || !msg.Author.IsBot)
        {
            await RespondAsync("You cannot use this command on this message.");

            return;
        }

        await RespondAsync("i gochu");

        for (var i = 0; i < 5; i++)
        {
            await msg.Channel.SendMessageAsync($"<@{PingTargetUserId}> GET UR ASS IN HERE AND CLAIM " + msg.GetLink());

            await Task.Delay(1000);
        }
    }
}
