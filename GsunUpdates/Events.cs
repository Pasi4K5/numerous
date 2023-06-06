using Discord;
using Discord.WebSocket;
using OpenAI_API;

namespace GsunUpdates;

public sealed class Events
{
    private readonly DiscordSocketClient _client;
    private readonly ChatBot _chatBot;
    private readonly MessageCategorizer _categorizer;

    private readonly string[] _preFilter =
    {
        "map", "osu", "roast", "gsun", "bot", "update"
    };

    public Events(DiscordSocketClient client, ChatBot chatBot, OpenAIAPI openAiApi)
    {
        _categorizer = new(openAiApi);
        _client = client;
        _chatBot = chatBot;

        _client.MessageReceived += HandleMessageReceived;
    }

    private async Task HandleMessageReceived(SocketMessage message)
    {
        if (message.Author.IsBot || message.Channel is IPrivateChannel || _chatBot.IsShutUp)
        {
            return;
        }

        var botWasMentioned = message.MentionedUsers.Select(x => x.Id).Contains(_client.CurrentUser.Id);

        if (botWasMentioned)
        {
            SendMessage();

            return;
        }

        if (
            _preFilter.Any(x => message.CleanContent.ToLower().Contains(x))
            && message.CleanContent.Count(char.IsLetter) > 10
            && await _categorizer.MessageIsDirectedAtBot(message)
        )
        {
            SendMessage();
        }

        void SendMessage()
        {
            using var _ = message.Channel.EnterTypingState();

            Task.Run(async () =>
            {
                var response = await _chatBot.GetResponse(message);

                if (response[..18].Contains(": "))
                {
                    response = response.Split(": ")[1];
                }

                foreach (var discordMessage in response.ToDiscordMessageStrings())
                {
                    await _chatBot.SendMessageAsync(message.Channel, discordMessage, message);
                }
            });
        }
    }
}
