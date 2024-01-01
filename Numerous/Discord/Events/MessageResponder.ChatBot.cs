// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;
using Numerous.Util;

namespace Numerous.Discord.Events;

public partial class MessageResponder
{
    private async Task RespondWithChatBotAsync(SocketMessage msg)
    {
        if (msg.Author.IsBot || msg.Channel is IPrivateChannel)
        {
            return;
        }

        var botWasMentioned = msg.MentionedUsers.Select(x => x.Id).Contains(client.CurrentUser.Id);

        if (!botWasMentioned)
        {
            return;
        }

        using var _ = msg.Channel.EnterTypingState();

        var (shouldRespond, response) = await openAi.GetResponseAsync(msg);

        if (!shouldRespond)
        {
            return;
        }

        foreach (var discordMessage in response.ToDiscordMessageStrings())
        {
            await msg.ReplyAsync(discordMessage);
        }
    }
}
