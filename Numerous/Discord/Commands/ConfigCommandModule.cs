// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.ApiClients.Osu;

namespace Numerous.Discord.Commands;

[Group("config", "Configuration commands")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public sealed class ConfigCommandModule : CommandModule
{
    [Group("role", "Role configuration commands")]
    public sealed class RoleCommandModule(OsuVerifier verifier) : CommandModule
    {
        [UsedImplicitly]
        [SlashCommand("set", "Configures which role to assign to which users.")]
        public async Task SetRole(
            [Summary("group", "The group to set the role for.")] OsuUserGroup group,
            [Summary("role", "The role to assign to users in the group.")] IRole role
        )
        {
            await verifier.SetRoleAsync(Context.Guild, group, role);

            await RespondWithEmbedAsync(
                $"Set role for group {group} to {role.Mention}.",
                ResponseType.Success
            );
        }

        [UsedImplicitly]
        [SlashCommand("remove", "Stops automatically assigning the given role.")]
        public async Task RemoveRole(
            [Summary("group", "The group to remove the role for.")] OsuUserGroup group
        )
        {
            await verifier.RemoveRoleAsync(Context.Guild, group);

            await RespondWithEmbedAsync(
                $"Removed role for group {group}.",
                ResponseType.Success
            );
        }
    }
}
