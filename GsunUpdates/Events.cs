using Discord;
using Discord.WebSocket;

namespace GsunUpdates;

public sealed class Events
{
    private readonly DiscordSocketClient _client;
    private readonly ChatBot _chatBot;

    public Events(DiscordSocketClient client, ChatBot chatBot)
    {
        _client = client;
        _chatBot = chatBot;

        _client.MessageReceived += HandleMessageReceived;
    }

    private Task HandleMessageReceived(SocketMessage message)
    {
        Task.Run(async () =>
        {
            if (!message.MentionedUsers.Select(x => x.Id).Contains(_client.CurrentUser.Id)
                || message.Channel is IPrivateChannel)
            {
                return;
            }

            foreach (var discordMessage in (await _chatBot.GetResponse(message)).ToDiscordMessageStrings())
            {
                await message.Channel.SendMessageAsync(discordMessage);
            }
        });

        return Task.CompletedTask;
    }
}
