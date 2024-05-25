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
        client.UserJoined += Greet;
    }

    private async Task Greet(SocketGuildUser user)
    {
        if ((await db.Users.FindByIdAsync(user.Id))?.OsuId is not null)
        {
            return;
        }

        var cmd = await cm.GetCommandMentionAsync<VerifyCommandModule>(nameof(VerifyCommandModule.Verify), user.Guild);

        var embed = new EmbedBuilder()
            .WithTitle("Welcome to Numerus!")
            .WithDescription(
                $"Use {cmd} to verify your osu! account and get your roles!"
            ).WithColor(Color.Green)
            .Build();

        var channelId = (await db.GuildOptions.FindOrInsertByIdAsync(user.Guild.Id)).JoinMessageChannel;

        if (channelId is null)
        {
            return;
        }

        var channel = user.Guild.GetTextChannel(channelId.Value);

        await channel.SendMessageAsync(user.Mention, embed: embed);
    }
}
