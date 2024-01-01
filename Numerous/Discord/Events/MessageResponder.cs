using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Numerous.ApiClients.OpenAi;
using Numerous.DependencyInjection;

namespace Numerous.Discord.Events;

[HostedService]
public sealed partial class MessageResponder(
    DiscordSocketClient client,
    OpenAiClient openAi
) : IHostedService, IDisposable
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.MessageReceived += async msg => await RespondAsync(msg);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task RespondAsync(SocketMessage msg)
    {
        if (!msg.Author.IsBot)
        {
            Task.Run(async () =>
            {
                if (await RespondToCommandAsync(msg) || await RespondToBanMessageAsync(msg))
                {
                    return;
                }

                await RespondWithChatBotAsync(msg);
            });
        }

        return Task.CompletedTask;
    }
}
