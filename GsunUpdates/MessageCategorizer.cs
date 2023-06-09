using Discord.WebSocket;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace GsunUpdates;

public sealed class MessageCategorizer
{
    private readonly string[] _preFilter =
    {
        "map", "roast", "gsun", "bot", "update", "news"
    };

    private readonly OpenAIAPI _api;
    private IChatEndpoint Chat => _api.Chat;

    public MessageCategorizer(OpenAIAPI api)
    {
        _api = api;
    }

    public async ValueTask<bool> MessageIsDirectedAtBot(SocketMessage message)
    {
        if (!_preFilter.Any(x => message.CleanContent.ToLower().Contains(x)))
        {
            return false;
        }

        var conversation = Chat.CreateConversation(new ChatRequest
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0,
            MaxTokens = 1
        });

        conversation.AppendSystemMessage(
            "You are the unfriendly Discord bot \"Not Gsun Updates\" (also known as \"Gsun Updates\", \"Gsun bot\", or similar) who roast osu! maps/beatmaps. "
            + "If the prompt's text has to do anything with you or roasting maps, you should respond with \"1\". "
            + "Otherwise, you should respond with \"0\"."
        );

        conversation.AppendUserInput("what does this gsun bot do?");
        conversation.AppendExampleChatbotOutput("1");

        conversation.AppendUserInput("what's going on with gsun?");
        conversation.AppendExampleChatbotOutput("0");

        conversation.AppendUserInput("i hate this weird gsun bot man");
        conversation.AppendExampleChatbotOutput("1");
        conversation.AppendUserInput(message.CleanContent);

        return await conversation.GetResponseFromChatbotAsync() == "1";
    }
}
