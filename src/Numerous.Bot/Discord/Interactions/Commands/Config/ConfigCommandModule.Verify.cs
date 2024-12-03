// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Discord.Events;
using Numerous.Database.Context;
using Numerous.Database.Dtos;

namespace Numerous.Bot.Discord.Interactions.Commands.Config;

public partial class ConfigCommandModule
{
    [Group("verify", "Verification configuration commands")]
    public sealed class VerifyCommandModule(IUnitOfWork uow) : InteractionModule
    {
        // TODO: Add command to remove join message
        [UsedImplicitly]
        [SlashCommand("set_join_message", "(Un-)Sets the channel and message to send to users as soon as they join.")]
        public async Task SetJoinMessageChannel(
            [Summary("channel", "The channel to send join messages to.")]
            ITextChannel channel,
            [Summary("title", "The title of the join message.")]
            string? title = null,
            [Summary("description", "The description of the join message.")]
            string? description = null
        )
        {
            await DeferAsync();

            var joinMessage = new JoinMessageDto
            {
                GuildId = Context.Guild.Id,
                ChannelId = channel.Id,
                Title = title,
                Description = description,
            };

            await uow.Guilds.SetJoinMessageAsync(joinMessage);

            await uow.CommitAsync();

            if (title is null && description is null)
            {
                await FollowupWithEmbedAsync(
                    message: "Please provide a title and/or description for the join message embed.",
                    type: ResponseType.Error
                );

                return;
            }

            await FollowupWithEmbedAsync(
                message: $"Set join message channel to {channel.Mention}.\n"
                         + $"Here is a preview of the message:",
                type: ResponseType.Success
            );

            await DiscordEventHandler.GreetAsync(Context.Guild.GetUser(Context.User.Id), joinMessage, Context.Channel);
        }

        [UsedImplicitly]
        [SlashCommand("set_verified_role", "(Un-)Sets the role which's users will be greeted as soon as the role is added/removed.")]
        public async Task SetMemberRole(
            [Summary("role", "The role which's users should be greeted as soon as the role is removed")]
            IRole? role = null,
            [Summary("on_added", "Whether users should be greeted once the role is added or removed.")]
            bool greetOnAdded = true
        )
        {
            await DeferAsync();

            await uow.Guilds.SetVerifiedRoleAsync(Context.Guild.Id, role?.Id, greetOnAdded);

            await uow.CommitAsync();

            await FollowupWithEmbedAsync(
                message: role is not null
                    ? $"Users will now be greeted as soon as the {role.Mention} role is {(greetOnAdded ? "assigned to" : "removed from")} them."
                    : "(Un-)Verified role removed. If the join message channel is set, all users will be greeted as soon as they join.",
                type: ResponseType.Success
            );
        }
    }
}
