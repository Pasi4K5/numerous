// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Database;

namespace Numerous.Bot.Discord.Interactions.Commands;

public partial class ConfigCommandModule
{
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
