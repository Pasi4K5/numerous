﻿// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Database.Context;

namespace Numerous.Bot.Discord.Interactions.Commands;

[CommandContextType(InteractionContextType.Guild)]
public sealed class ToggleReadOnlyCommandModule(IUnitOfWork uow) : InteractionModule
{
    [UsedImplicitly]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("togglereadonly", "Makes the given channel read-only.")]
    public async Task ToggleReadOnly(
        [Summary("channel", "The channel to make read-only.")] ITextChannel channel
    )
    {
        var isReadOnly = await uow.MessageChannels.ToggleReadOnlyAsync(channel.GuildId, channel.Id);

        await uow.CommitAsync();

        await RespondWithEmbedAsync(
            message: $"Channel {channel.Mention} is now {(isReadOnly ? "read-only" : "writable")}.",
            type: ResponseType.Success
        );
    }
}
