// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;
using Numerous.Bot.Discord.Interactions.Commands;
using Numerous.Bot.Util;

namespace Numerous.Bot.Discord.Events;

public partial class DiscordEventHandler
{
    [Init]
    private void Greet_Init()
    {
        client.UserJoined += async u => await GreetAsync(u);
        client.GuildMemberUpdated += GreetAsync;
    }

    private async Task GreetAsync(Cacheable<SocketGuildUser, ulong> oldUser, SocketGuildUser newUser)
    {
        var options = await db.GuildOptions.FindOrInsertByIdAsync(newUser.Guild.Id);
        var channelId = options.JoinMessageChannel;
        var roleId = options.UnverifiedRole;

        if (roleId is null
            || !oldUser.HasValue
            || oldUser.Value.Roles.All(r => r.Id != roleId)
            || newUser.Roles.Any(r => r.Id == roleId)
            // "1 hour" magic number could be replaced with a configurable option
            /*|| (newUser.JoinedAt is not null && newUser.JoinedAt.Value.AddHours(1) > DateTimeOffset.UtcNow)*/)
        {
            return;
        }

        await GreetAsync(newUser, channelId);
    }

    private async Task GreetAsync(SocketGuildUser user)
    {
        var options = await db.GuildOptions.FindOrInsertByIdAsync(user.Guild.Id);
        var channelId = options.JoinMessageChannel;
        var roleId = options.UnverifiedRole;

        if (roleId is not null)
        {
            return;
        }

        await GreetAsync(user, channelId);
    }

    private async Task GreetAsync(SocketGuildUser user, ulong? channelId)
    {
        if (user.IsBot || channelId is null)
        {
            return;
        }

        var cmd = await cm.GetCommandMentionAsync<VerifyCommandModule>(nameof(VerifyCommandModule.Verify), user.Guild);

        var embed = new EmbedBuilder()
            .WithTitle($"Welcome to {user.Guild.Name}!")
            .WithDescription(
                $"Use {cmd} to verify your osu! account and get your roles!"
            ).WithColor(Color.Green)
            .Build();

        var channel = user.Guild.GetTextChannel(channelId.Value);

        await channel.SendMessageAsync(user.Mention, embed: embed);
    }
}
