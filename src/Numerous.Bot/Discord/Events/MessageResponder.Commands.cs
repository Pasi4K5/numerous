// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;

namespace Numerous.Bot.Discord.Events;

public partial class MessageResponder
{
    private const ulong WhithardUserId = 357477071179481100;
    private const int Repetitions = 10;
    private const int Delay = 1000;

    private static async Task<bool> RespondToCommandMessageAsync(SocketMessage msg)
    {
        if (msg.CleanContent != "!whit" || msg.Channel is not IGuildChannel)
        {
            return false;
        }

        var msgIds = new List<ulong>();

        for (var i = 0; i < Repetitions; i++)
        {
            var newMsg = await msg.Channel.SendMessageAsync($"<@{WhithardUserId}>");
            msgIds.Add(newMsg.Id);
            await Task.Delay(Delay);
        }

        await Task.WhenAll(msgIds.Select(id => msg.Channel.DeleteMessageAsync(id)));

        return true;
    }
}
