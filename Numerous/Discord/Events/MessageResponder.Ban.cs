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
