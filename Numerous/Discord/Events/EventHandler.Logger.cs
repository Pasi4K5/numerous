// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.WebSocket;
using Numerous.Util;
using Serilog;

namespace Numerous.Discord.Events;

public partial class DiscordEventHandler
{
    [Init]
    private void Logger_Init()
    {
        _client.SlashCommandExecuted += LogSlashCommand;
        _client.InteractionCreated += LogInteraction;
    }

    private async Task LogSlashCommand(SocketSlashCommand cmd)
    {
        var guild = cmd.GuildId is not null
            ? await _client.Rest.GetGuildAsync(cmd.GuildId.Value)
            : null;

        Log.Information(
            """User "{User}" (ID: {Uid}) executed slash command "{Cmd}" with args {Args} in channel "{Channel}" (ID: {ChannelId}) in guild "{Guild}" (ID: {GuildId})""",
            cmd.User.Username,
            cmd.User.Id,
            cmd.CommandName,
            cmd.Data.Options.ToLogString(),
            cmd.Channel,
            cmd.ChannelId,
            guild?.Name ?? "null",
            cmd.GuildId);
    }

    private Task LogInteraction(SocketInteraction interaction)
    {
        if (interaction is SocketMessageCommand msgCmd)
        {
            Log.Information(
                """User "{User}" (ID: {Uid}) executed message command "{Cmd}" on message <{Msg}>.""",
                msgCmd.User.Username,
                msgCmd.User.Id,
                msgCmd.CommandName,
                msgCmd.Data.Message.GetLink());
        }

        return Task.CompletedTask;
    }
}
