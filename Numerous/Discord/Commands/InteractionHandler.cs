using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Numerous.Configuration;
using Numerous.DependencyInjection;

namespace Numerous.Discord.Commands;

[HostedService]
public sealed class InteractionHandler(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    ConfigManager configManager
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var cfg = configManager.Get();

        client.Ready += cfg.DevMode
            ? async () => await interactions.RegisterCommandsToGuildAsync(cfg.DevGuildId)
            : async () => await interactions.RegisterCommandsGloballyAsync();
        client.InteractionCreated += OnInteractionCreatedAsync;

        await interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), services);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        interactions.Dispose();

        return Task.CompletedTask;
    }

    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(client, interaction);
            var result = await interactions.ExecuteCommandAsync(context, services);

            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ToString());
            }
        }
        catch
        {
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(msg => msg.Result.DeleteAsync());
            }
        }
    }
}
