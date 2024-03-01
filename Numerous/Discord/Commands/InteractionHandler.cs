// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

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

        client.Ready += cfg.GuildMode
            ? async () =>
            {
                foreach (var cmd in await client.GetGlobalApplicationCommandsAsync())
                {
                    await cmd.DeleteAsync();
                }

                foreach (var guildId in cfg.GuildIds)
                {
                    await interactions.RegisterCommandsToGuildAsync(guildId);
                }
            }
            : async () =>
            {
                foreach (var guild in await client.Rest.GetGuildsAsync())
                {
                    await guild.DeleteSlashCommandsAsync();
                }

                await interactions.RegisterCommandsGloballyAsync();
            };
        client.InteractionCreated += OnInteractionCreatedAsync;

        await interactions.AddModulesAsync(Assembly.GetEntryAssembly(), services);
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
            await interactions.ExecuteCommandAsync(context, services);
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
