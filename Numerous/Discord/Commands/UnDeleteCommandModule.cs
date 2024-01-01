// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using MongoDB.Driver;
using Numerous.Database;

namespace Numerous.Discord.Commands;

[UsedImplicitly]
public sealed class UnDeleteCommandModule(DbManager db) : CommandModule
{
    [UsedImplicitly]
    [SlashCommand("undelete", "Reveals the last deleted message in this channel.")]
    public async Task UnDelete()
    {
        await DeferAsync();

        var message = await db.DiscordMessages
            .Find(m => m.ChannelId == Context.Channel.Id && m.DeletedAt != null)
            .SortByDescending(m => m.DeletedAt)
            .FirstOrDefaultAsync();

        if (message is null)
        {
            await FollowupAsync("No deleted messages found in this channel.");

            return;
        }

        var user = await Context.Client.Rest.GetUserAsync(message.AuthorId);

        var embed = new EmbedBuilder()
            .WithAuthor(user.Username, user.GetAvatarUrl())
            .WithDescription($"**User ID:** {message.AuthorId}\n"
                             + $"**Message ID:** {message.Id}")
            .WithColor(new(0xff0000))
            .WithFields(
                new EmbedFieldBuilder()
                    .WithName("Message text")
                    .WithValue(message.Contents.Last()),
                new EmbedFieldBuilder()
                    .WithName("Sent")
                    .WithValue($"<t:{message.Timestamps.First().ToUnixTimeSeconds()}:R>")
                    .WithIsInline(true),
                new EmbedFieldBuilder()
                    .WithName("Deleted")
                    .WithValue($"<t:{message.DeletedAt!.Value.ToUnixTimeSeconds()}:R>")
                    .WithIsInline(true)
            )
            .Build();

        await FollowupAsync(embed: embed);
    }
}
