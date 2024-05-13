// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using Numerous.Bot.Database;
using Numerous.Bot.Discord.Commands;

namespace Numerous.Bot.Discord.Interactions.Commands;

public sealed class UnEditCommandModule(IDbService db, DiscordSocketClient client) : InteractionModule
{
    [UsedImplicitly]
    [MessageCommand("Unedit")]
    public async Task UnEdit(IMessage msg)
    {
        await UnEdit(msg.Id.ToString());
    }

    [UsedImplicitly]
    [SlashCommand("unedit", "Reveals all versions of the given message.")]
    public async Task UnEdit(
        [Summary("message", "The message to reveal. Can be a message ID or a link.")]
        string msg
    )
    {
        await DeferAsync();

        ulong? messageId = msg switch
        {
            _ when ulong.TryParse(msg, out var id) => id,
            _ when msg.StartsWith("https://discord.com/channels/", StringComparison.Ordinal) => ulong.Parse(
                msg.Split('/').Last()),
            _ => null,
        };

        if (messageId is null)
        {
            await FollowupAsync("Invalid message ID or link.");

            return;
        }

        var discordMessage = await db.DiscordMessages.FindByIdAsync(messageId.Value);

        if (discordMessage is null || discordMessage.IsHidden)
        {
            await FollowupAsync("Message not found.");

            return;
        }

        var user = await client.Rest.GetUserAsync(discordMessage.AuthorId);

        var embed = new EmbedBuilder()
            .WithAuthor(user.Username, user.GetAvatarUrl())
            .WithDescription($"**User ID:** {discordMessage.AuthorId}")
            .WithColor(new(0xff0000))
            .WithFields(
                discordMessage.Contents.Zip(discordMessage.Timestamps)
                    .Select((x, i) => new EmbedFieldBuilder()
                        .WithName($"Version {i + 1}")
                        .WithValue(x.First + $"\n<t:{x.Second.ToUnixTimeSeconds()}:R>"))
            ).Build();

        await FollowupAsync(embed: embed);
    }
}
