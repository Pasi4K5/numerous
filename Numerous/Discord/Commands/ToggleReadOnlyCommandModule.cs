// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using MongoDB.Driver;
using Numerous.Database;
using Numerous.Database.Entities;

namespace Numerous.Discord.Commands;

public sealed class ToggleReadOnlyCommandModule(DbManager db) : CommandModule
{
    [UsedImplicitly]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("togglereadonly", "Makes the given channel read-only.")]
    public async Task ToggleReadOnly(
        [Summary("channel", "The channel to make read-only.")] ITextChannel channel
    )
    {
        var guildOptions = await (await db.GuildOptions
                .FindAsync(x => x.Id == Context.Guild.Id)
            ).FirstOrDefaultAsync();

        if (guildOptions.ReadOnlyChannels.Contains(channel.Id))
        {
            await db.GuildOptions.UpdateOneAsync(
                x => x.Id == Context.Guild.Id,
                Builders<GuildOptions>.Update.Pull(x => x.ReadOnlyChannels, channel.Id)
            );
        }
        else
        {
            await db.GuildOptions.UpdateOneAsync(
                x => x.Id == Context.Guild.Id,
                Builders<GuildOptions>.Update.Push(x => x.ReadOnlyChannels, channel.Id)
            );
        }

        await RespondWithEmbedAsync(
            $"Channel {channel.Mention} is now {(guildOptions.ReadOnlyChannels.Contains(channel.Id) ? "writable" : "read-only")}.",
            type: ResponseType.Success
        );
    }
}
