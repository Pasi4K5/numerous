// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Database.Context;

namespace Numerous.Bot.Discord.Interactions.Commands.Config;

public partial class ConfigCommandModule
{
    [Group("verify", "Verification configuration commands")]
    public sealed class VerifyCommandModule(IUnitOfWork uow) : InteractionModule
    {
        [UsedImplicitly]
        [SlashCommand("set_member_role", "(Un-)Sets the role users will be assigned after verification.")]
        public async Task SetMemberRole
        (
            [Summary("role", "The role which's users should be greeted as soon as the role is removed")]
            IRole? role = null
        )
        {
            await DeferAsync();

            await uow.Guilds.SetVerifiedRoleAsync(Context.Guild.Id, role?.Id);

            await uow.CommitAsync();

            await FollowupWithEmbedAsync(
                message: role is not null
                    ? $"Member role set to {role.Mention}."
                    : "Member role removed.",
                type: ResponseType.Success
            );
        }
    }
}
