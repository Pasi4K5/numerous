// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.ApiClients.Osu;
using Numerous.Bot.Database;
using Numerous.Bot.Database.Entities;
using Numerous.Bot.Discord.Events;

namespace Numerous.Bot.Discord.Interactions.Commands;

[UsedImplicitly]
[Group("config", "Configuration commands")]
[DefaultMemberPermissions(GuildPermission.Administrator)]
public sealed class ConfigCommandModule : InteractionModule
{
    [Group("role", "Role configuration commands")]
    public sealed class RoleCommandModule(OsuVerifier verifier) : InteractionModule
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
    public sealed class VerifyCommandModule(IDbService db, DiscordEventHandler eh) : InteractionModule
    {
        [UsedImplicitly]
        [SlashCommand("set_join_message", "(Un-)Sets the channel and message to send to users as soon as they join.")]
        public async Task SetJoinMessageChannel(
            [Summary("channel", "The channel to send join messages to.")]
            ITextChannel? channel = null,
            [Summary("title", "The title of the join message.")]
            string? title = null,
            [Summary("description", "The description of the join message.")]
            string? description = null
        )
        {
            await DeferAsync();

            var joinMessage = channel is not null
                ? new GuildOptions.DbJoinMessage(channel.Id, title, description)
                : null;

            await db.GuildOptions.UpdateByIdAsync(
                Context.Guild.Id,
                x => x.JoinMessage, joinMessage
            );

            await FollowupWithEmbedAsync(
                message: channel is not null
                    ? $"Set join message channel to {channel.Mention}.\n"
                      + $"Here is a preview of the message:"
                    : "Removed join message channel.",
                type: ResponseType.Success
            );

            if (channel is not null)
            {
                await eh.GreetAsync(Context.Guild.GetUser(Context.User.Id), joinMessage, Context.Channel);
            }
        }

        [UsedImplicitly]
        [SlashCommand("set_unverified_role", "(Un-)Sets the role which's users will be greeted as soon as the role is removed.")]
        public async Task SetMemberRole(
            [Summary("role", "The role which's users should be greeted as soon as the role is removed")]
            IRole? role = null
        )
        {
            await DeferAsync();

            await db.GuildOptions.UpdateByIdAsync(Context.Guild.Id, x => x.UnverifiedRole, role?.Id);

            await FollowupWithEmbedAsync(
                message: role is not null
                    ? $"Users will now be greeted as soon as the {role.Mention} role is removed from them."
                    : "Unverified role removed. If the join message channel is set, all users will be greeted as soon as they join.",
                type: ResponseType.Success
            );
        }
    }

    [Group("deletedmessages", "Deleted messages configuration commands")]
    public sealed class DeletedMessagesCommandModule(IDbService db) : InteractionModule
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
