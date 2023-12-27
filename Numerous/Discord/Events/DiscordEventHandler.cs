using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Numerous.ApiClients.OpenAi;
using Numerous.Database;
using Numerous.DependencyInjection;
using Numerous.Util;

namespace Numerous.Discord.Events;

[HostedService]
public sealed partial class DiscordEventHandler : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly OpenAiClient _openAiClient;
    private readonly DbManager _db;

    public DiscordEventHandler(
        DiscordSocketClient client,
        OpenAiClient openAiClient,
        DbManager db)
    {
        _client = client;
        _openAiClient = openAiClient;
        _db = db;

        this.Init();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
