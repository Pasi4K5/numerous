using Discord;
using Discord.WebSocket;

namespace GsunUpdates;

public class Events
{
    private readonly DiscordSocketClient _client;

    public Events(DiscordSocketClient client)
    {
        _client = client;

        _client.MessageReceived += HandleMessageReceived;
    }

    private async Task HandleMessageReceived(SocketMessage message)
    {
        if (!message.MentionedUsers.Select(x => x.Id).Contains(_client.CurrentUser.Id))
        {
            return;
        }

        if (message.Author.Id == 417791382305112064 /* <- eri */)
        {
            await message.Channel.SendMessageAsync(
                "fuck you :3",
                messageReference: new MessageReference(message.Id)
            );
        }
        else
        {
            await message.Channel.SendMessageAsync(
                "GSUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUN!",
                messageReference: new MessageReference(message.Id)
            );
        }
    }
}
