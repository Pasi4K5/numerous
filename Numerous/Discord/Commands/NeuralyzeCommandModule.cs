using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.ApiClients.OpenAi;

namespace Numerous.Discord.Command;

public sealed class NeuralyzeCommandModule(OpenAiClient ai) : CommandModule
{
    [UsedImplicitly]
    [SlashCommand("neuralyze", "Starts a new conversation.")]
    public async Task Neuralyze()
    {
        ai.RestartConversation();

        await RespondAsync("I forgor \ud83d\udc80");
    }
}
