using System.Text.RegularExpressions;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Numerous.Util;

namespace Numerous.Discord.Events;

public partial class DiscordEventHandler
{
    private static readonly string[] _banPhrases =
    {
        "chat kill {user} with hammers",
        "chat impregnate {user}",
    };

    [Init]
    private void MessageResponder_Init()
    {
        _client.MessageReceived += async msg => await MessageResponder_RespondAsync(msg);
    }

    private async Task MessageResponder_RespondAsync(SocketMessage msg)
    {
        if (await MessageResponder_RespondToBanMessageAsync(msg))
        {
            return;
        }

        await MessageResponder_RespondWithChatBotAsync(msg);
    }

    private async Task<bool> MessageResponder_RespondToBanMessageAsync(SocketMessage msg)
    {
        var idToBan = MessageResponder_GetUserIdToBan(msg.Content);

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

        if (target.Id == _client.CurrentUser.Id
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

    private static ulong? MessageResponder_GetUserIdToBan(string msg)
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

    private async Task MessageResponder_RespondWithChatBotAsync(SocketMessage msg)
    {
        if (msg.Author.IsBot || msg.Channel is IPrivateChannel)
        {
            return;
        }

        var botWasMentioned = msg.MentionedUsers.Select(x => x.Id).Contains(_client.CurrentUser.Id);

        if (!botWasMentioned)
        {
            return;
        }

        using var _ = msg.Channel.EnterTypingState();

        var (shouldRespond, response) = await _openAiClient.GetResponseAsync(msg);

        if (!shouldRespond)
        {
            return;
        }

        foreach (var discordMessage in response.ToDiscordMessageStrings())
        {
            await msg.ReplyAsync(discordMessage);
        }
    }
}
