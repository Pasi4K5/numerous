using Discord.WebSocket;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace GsunUpdates;

public sealed class MessageCategorizer
{
    private readonly OpenAIAPI _api;
    private readonly Conversation _conversation;
    private IChatEndpoint Chat => _api.Chat;

    public MessageCategorizer(OpenAIAPI api)
    {
        _api = api;
        _conversation = Chat.CreateConversation(new ChatRequest
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0,
            MaxTokens = 100
        });

        _conversation.AppendSystemMessage(
            "You are the unfriendly Discord bot \"Not Gsun Updates\" (also known as \"Gsun Updates\", \"Gsun bot\", or similar) who roast osu! maps/beatmaps. "
            + "If the promt's text has to do anything with you or roasting maps, you should respond with \"1\". "
            + "Otherwise, you should respond with \"0\"."
        );

        _conversation.AppendUserInput("what does this gsun bot do?");
        _conversation.AppendExampleChatbotOutput("1");

        _conversation.AppendUserInput("what's going on with gsun?");
        _conversation.AppendExampleChatbotOutput("0");

        _conversation.AppendUserInput("i hate this weird gsun bot man");
        _conversation.AppendExampleChatbotOutput("1");
    }

    public async Task<bool> MessageIsDirectedAtBot(SocketMessage message)
    {
        _conversation.AppendUserInput(message.CleanContent);

        return await _conversation.GetResponseFromChatbotAsync() == "1";
    }
}
