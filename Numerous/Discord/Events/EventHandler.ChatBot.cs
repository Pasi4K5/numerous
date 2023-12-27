using Discord;
using Discord.WebSocket;
using Numerous.Util;

namespace Numerous.Discord.Events;

public partial class DiscordEventHandler
{
    [Init]
    private void ChatBot_Init()
    {
        _client.MessageReceived += async msg => await ChatBot_RespondAsync(msg);
    }

    private async Task ChatBot_RespondAsync(SocketMessage message)
    {
        if (message.Author.IsBot || message.Channel is IPrivateChannel)
        {
            return;
        }

        var botWasMentioned = message.MentionedUsers.Select(x => x.Id).Contains(_client.CurrentUser.Id);

        if (!botWasMentioned)
        {
            return;
        }

        using var _ = message.Channel.EnterTypingState();

        var (shouldRespond, response) = await _openAiClient.GetResponseAsync(message);

        if (!shouldRespond)
        {
            return;
        }

        foreach (var discordMessage in response.ToDiscordMessageStrings())
        {
            await message.ReplyAsync(discordMessage);
        }
    }
}
