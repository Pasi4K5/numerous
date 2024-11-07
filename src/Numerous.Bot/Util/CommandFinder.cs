// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.Interactions;
using Discord.WebSocket;
using Numerous.Common.Config;

namespace Numerous.Bot.Util;

public sealed class CommandFinder(InteractionService interactions, IConfigProvider cfg)
{
    public async Task<string> GetCommandMentionAsync<TModule>(
        string methodName,
        SocketGuild guild
    ) where TModule : class
    {
        var (name, id) = await GetCommandInfoAsync<TModule>(methodName, guild);

        return id is null ? string.Empty : $"</{name}:{id}>";
    }

    private async Task<(string? name, ulong? id)> GetCommandInfoAsync<TModule>(
        string methodName,
        SocketGuild guild
    ) where TModule : class
    {
        var cmdInfo = interactions.GetSlashCommandInfo<TModule>(methodName);

        var cmdId = (await guild.GetApplicationCommandsAsync()).FirstOrDefault(cmd =>
            // TODO: Try to find a better way than this.
            cmd.Name == cmdInfo.Name
            && (!cmd.IsGlobalCommand || cmd.Guild.Id == guild.Id)
            && cmd.IsNsfw == cmdInfo.IsNsfw
            && cmd.IsDefaultPermission == cmdInfo.DefaultPermission
            && cmd.Description == cmdInfo.Description
            && cmd.ApplicationId == cfg.Get().DiscordClientId
            && cmd.Type == cmdInfo.CommandType
            && cmd.ContextTypes?.SequenceEqual(cmdInfo.ContextTypes) != false
            && cmd.Options.Count == cmdInfo.Parameters.Count
        )?.Id;

        return (cmdInfo.Name, cmdId);
    }
}
