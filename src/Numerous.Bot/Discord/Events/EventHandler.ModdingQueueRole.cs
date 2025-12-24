// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Numerous.Bot.Discord.Util;
using Numerous.Bot.Util;

namespace Numerous.Bot.Discord.Events;

public partial class DiscordEventHandler
{
    [Init]
    private void ModdingQueueRole_Init()
    {
        var cfg = configProvider.Get();
        var channelId = cfg.ModdingQueue.ChannelId;
        var logChannelId = cfg.ModdingQueue.LogChannelId;
        var roleId = cfg.ModdingQueue.RoleId;
        var link = cfg.BaseUrl;

        ddnClient.MessageReceived += async message =>
        {
            if (
                message.Channel is not IGuildChannel channel
                || channel.Id != channelId
                || message.Author.IsBot
            )
            {
                return;
            }

            var user = await channel.Guild.GetUserAsync(message.Author.Id);
            var logChannel = await channel.Guild.GetTextChannelAsync(logChannelId);

            switch (message.Content.ToLower())
            {
                case "!im-goated":
                    if (!await verifier.UserIsVerifiedAsync(user.Id))
                    {
                        await message.ReplyAsync(
                            "You need link your osu! account to join the modding queue. "
                            + $"Click {"here".ToMdLink(link)} to do that."
                        );

                        break;
                    }

                    if (user.RoleIds.Contains(roleId))
                    {
                        await message.ReplyAsync("You are already a member of the modding queue.");

                        break;
                    }

                    await user.AddRoleAsync(channel.Guild.GetRole(roleId));
                    await logChannel.SendMessageAsync(
                        $"{Emoji.Parse(":green_square:")} {user.Mention} has joined the modding queue."
                    );
                    await message.AddReactionAsync(Emoji.Parse(":white_check_mark:"));

                    break;
                case "!i-suck":
                    if (!user.RoleIds.Contains(roleId))
                    {
                        await message.ReplyAsync("You are not a member of the modding queue.");

                        break;
                    }

                    await user.RemoveRoleAsync(channel.Guild.GetRole(roleId));
                    await logChannel.SendMessageAsync(
                        $"{Emoji.Parse(":red_square:")} {user.Mention} has left the modding queue."
                    );
                    await message.AddReactionAsync(Emoji.Parse(":white_check_mark:"));

                    break;
            }
        };
    }
}
