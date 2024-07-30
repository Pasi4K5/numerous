// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.Interactions;
using Discord.WebSocket;
using Numerous.Common.Services;

namespace Numerous.Bot.Util;

public sealed class CommandFinder(InteractionService interactions, IConfigService cfg)
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
        var verifyCmd = interactions.GetSlashCommandInfo<TModule>(methodName);

        var cmdId = (await guild.GetApplicationCommandsAsync()).FirstOrDefault(cmd =>
            // TODO: Try to find a better way than this.
            cmd.Name == verifyCmd.Name
            && (!cmd.IsGlobalCommand || cmd.Guild.Id == guild.Id)
            && cmd.IsNsfw == verifyCmd.IsNsfw
            && cmd.IsDefaultPermission == verifyCmd.DefaultPermission
            && cmd.Description == verifyCmd.Description
            && cmd.ApplicationId == cfg.Get().DiscordClientId
            && cmd.Type == verifyCmd.CommandType
            && cmd.ContextTypes?.SequenceEqual(verifyCmd.ContextTypes) != false
            && cmd.Options.Count == verifyCmd.Parameters.Count
        )?.Id;

        return (verifyCmd.Name, cmdId);
    }
}
