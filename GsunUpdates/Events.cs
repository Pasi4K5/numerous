using Discord;
using Discord.WebSocket;
using OpenAI_API;

namespace GsunUpdates;

public sealed class Events
{
    private readonly DiscordSocketClient _client;
    private readonly ChatBot _chatBot;
    private readonly MessageCategorizer _categorizer;

    public Events(DiscordSocketClient client, ChatBot chatBot, OpenAIAPI openAiApi)
    {
        _categorizer = new(openAiApi);
        _client = client;
        _chatBot = chatBot;

        _client.MessageReceived += HandleMessageReceived;
    }

    private Task HandleMessageReceived(SocketMessage message)
    {
        Task.Run(async () =>
        {
            if (message.Channel is IPrivateChannel || message.Author.IsBot)
            {
                return;
            }

            var botWasMentioned = message.MentionedUsers.Select(x => x.Id).Contains(_client.CurrentUser.Id);

            if (botWasMentioned)
            {
                await SendMessage();
            }

            if (message.CleanContent.Count(char.IsLetter) > 10 && await _categorizer.MessageIsDirectedAtBot(message))
            {
                await SendMessage();
            }

            async Task SendMessage()
            {
                using var _ = message.Channel.EnterTypingState();

                var response = await _chatBot.GetResponse(message);

                if (response[..18].Contains(": "))
                {
                    response = response.Split(": ")[1];
                }

                foreach (var discordMessage in response.ToDiscordMessageStrings())
                {
                    await message.Channel.SendMessageAsync(discordMessage, messageReference: new MessageReference(message.Id));
                }
            }
        });

        return Task.CompletedTask;
    }
}
