// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.ApiClients.Osu;
using Numerous.Bot.Database;

namespace Numerous.Bot.Discord.Commands;

[UsedImplicitly]
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
            await verifier.LinkRoleAsync(Context.Guild, group, role);

            await RespondWithEmbedAsync(
                $"Set role for group {group} to {role.Mention}.",
                type: ResponseType.Success
            );

            await verifier.AssignAllRolesAsync();
        }

        [UsedImplicitly]
        [SlashCommand("remove", "Stops automatically assigning the given role.")]
        public async Task RemoveRole(
            [Summary("group", "The group to remove the role for.")] OsuUserGroup group
        )
        {
            await verifier.UnlinkRoleAsync(Context.Guild, group);

            await RespondWithEmbedAsync(
                $"Removed role for group {group}.",
                type: ResponseType.Success
            );

            await verifier.AssignAllRolesAsync();
        }
    }

    [Group("verify", "Verification configuration commands")]
    public sealed class VerifyCommandModule(OsuVerifier verifier) : CommandModule
    {
        [UsedImplicitly]
        [SlashCommand("setlogchannel", "Sets the channel to log verifications to.")]
        public async Task SetLogChannel(
            [Summary("channel", "The channel to log verifications to.")] ITextChannel channel
        )
        {
            await DeferAsync();

            await verifier.SetVerificationLogChannelAsync(Context.Guild, channel);

            await FollowupWithEmbedAsync(
                $"Set verification log channel to {channel.Mention}.",
                type: ResponseType.Success
            );
        }

        [UsedImplicitly]
        [SlashCommand("unsetlogchannel", "Unsets the channel to log verifications to.")]
        public async Task RemoveLogChannel()
        {
            await DeferAsync();

            await verifier.SetVerificationLogChannelAsync(Context.Guild, null);

            await FollowupWithEmbedAsync(
                "Removed verification log channel.",
                type: ResponseType.Success
            );
        }
    }

    [Group("deletedmessages", "Deleted messages configuration commands")]
    public sealed class DeletedMessagesCommandModule(IDbService db) : CommandModule
    {
        [UsedImplicitly]
        [SlashCommand("setchannel", "Sets the channel to log deleted messages to.")]
        public async Task SetChannel(
            [Summary("channel", "The channel to log deleted messages to.")] ITextChannel channel
        )
        {
            await DeferAsync();

            await db.GuildOptions.SetDeletedMessagesChannel(Context.Guild.Id, channel.Id);

            await FollowupWithEmbedAsync(
                $"Set deleted messages channel to {channel.Mention}.",
                type: ResponseType.Success
            );
        }

        [UsedImplicitly]
        [SlashCommand("unsetchannel", "Unsets the channel to log deleted messages to.")]
        public async Task RemoveChannel()
        {
            await DeferAsync();

            await db.GuildOptions.SetDeletedMessagesChannel(Context.Guild.Id, null);

            await FollowupWithEmbedAsync(
                "Removed deleted messages channel.",
                type: ResponseType.Success
            );
        }
    }
}
