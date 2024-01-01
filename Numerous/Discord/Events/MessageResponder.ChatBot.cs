using Discord;
using Discord.WebSocket;
using Numerous.Util;

namespace Numerous.Discord.Events;

public partial class MessageResponder
{
    private async Task RespondWithChatBotAsync(SocketMessage msg)
    {
        if (msg.Author.IsBot || msg.Channel is IPrivateChannel)
        {
            return;
        }

        var botWasMentioned = msg.MentionedUsers.Select(x => x.Id).Contains(client.CurrentUser.Id);

        if (!botWasMentioned)
        {
            return;
        }

        using var _ = msg.Channel.EnterTypingState();

        var (shouldRespond, response) = await openAi.GetResponseAsync(msg);

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
