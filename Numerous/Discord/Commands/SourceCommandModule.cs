using Discord.Interactions;
using JetBrains.Annotations;

namespace Numerous.Discord.Commands;

public sealed class SourceCommandModule : CommandModule
{
    private const string SourceUrl = "https://github.com/Pasi4K5/numerous";

    [UsedImplicitly]
    [SlashCommand("source", "Links to the source code.")]
    public async Task Source()
    {
        await RespondAsync("I am open source! You can find my source code here:\n" + SourceUrl);
    }
}
