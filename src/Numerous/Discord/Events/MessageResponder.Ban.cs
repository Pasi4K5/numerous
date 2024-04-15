// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Numerous.Util;

namespace Numerous.Discord.Events;

public partial class MessageResponder
{
    private static readonly string[] _banPhrases =
    {
        "chat kill {user} with hammers",
        "chat impregnate {user}",
    };

    private async Task<bool> RespondToBanMessageAsync(SocketMessage msg)
    {
        var idToBan = GetUserIdToBan(msg.Content);

        if (idToBan is null || msg.Channel is not IGuildChannel channel)
        {
            return false;
        }

        var target = await channel.Guild.GetUserAsync(idToBan.Value);

        if (target is null)
        {
            return true;
        }

        var sender = await channel.Guild.GetUserAsync(msg.Author.Id);
        var senderRolePositions = sender.RoleIds.Select(x => channel.Guild.GetRole(x).Position).Concat(new[] { -1 });
        var targetRolePositions = target.RoleIds.Select(x => channel.Guild.GetRole(x).Position).Concat(new[] { -1 });

        if (target.Id == client.CurrentUser.Id
            || target.Id == sender.Id
            || (channel.Guild.OwnerId != sender.Id
                && (sender.GuildPermissions.BanMembers != true || senderRolePositions.Max() <= targetRolePositions.Max())
            )
            || sender.IsBot
           )
        {
            await msg.ReplyAsync("fuck u");

            return true;
        }

        if (!(await channel.Guild.GetCurrentUserAsync()).GuildPermissions.BanMembers)
        {
            await msg.ReplyAsync("I don't have permission to ban members.");
        }
        else
        {
            try
            {
                await target.BanAsync(reason: "impregnated");

                await msg.ReplyAsync("he gone");
            }
            catch (HttpException)
            {
                await msg.ReplyAsync("I don't have permission to ban this user.");
            }
        }

        return true;
    }

    private static ulong? GetUserIdToBan(string msg)
    {
        var userMentionRegex = new Regex("<@.*?>");

        if (!userMentionRegex.IsMatch(msg))
        {
            return null;
        }

        var cleanMsg = msg.Replace(userMentionRegex.Match(msg).Value, "{user}").RemoveAll(",.?!:;");

        if (!ulong.TryParse(userMentionRegex.Match(msg).Value[2..^1], out var id)
            || _banPhrases.All(x => x.ToLower().Replace(" ", "") != cleanMsg.ToLower().Replace(" ", "")))
        {
            return null;
        }

        return id;
    }
}
