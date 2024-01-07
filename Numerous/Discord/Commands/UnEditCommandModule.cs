﻿// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using MongoDB.Driver;
using Numerous.Database;

namespace Numerous.Discord.Commands;

public sealed class UnEditCommandModule(DbManager db, DiscordSocketClient client) : CommandModule
{
    [UsedImplicitly]
    [SlashCommand("unedit", "Reveals all versions of the given message.")]
    public async Task UnEdit(
        [Summary("message", "The message to reveal. Can be a message ID or a link.")]
        string message
    )
    {
        await DeferAsync();

        ulong? messageId = message switch
        {
            _ when ulong.TryParse(message, out var id) => id,
            _ when message.StartsWith("https://discord.com/channels/", StringComparison.Ordinal) => ulong.Parse(
                message.Split('/').Last()),
            _ => null,
        };

        if (messageId is null)
        {
            await FollowupAsync("Invalid message ID or link.");

            return;
        }

        var discordMessage = await db.DiscordMessages.Find(m => m.Id == messageId).FirstOrDefaultAsync();

        if (discordMessage is null)
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
            )
            .Build();

        await FollowupAsync(embed: embed);
    }
}