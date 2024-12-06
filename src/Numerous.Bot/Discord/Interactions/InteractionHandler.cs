// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Numerous.Bot.Discord.Util;
using Numerous.Common;
using Numerous.Common.Config;
using Numerous.Common.Services;
using Numerous.Common.Util;
using Serilog;

namespace Numerous.Bot.Discord.Interactions;

public sealed class InteractionHandler(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    IConfigProvider cfgProvider
) : HostedService
{
    private Config Cfg => cfgProvider.Get();

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        interactions.InteractionExecuted += HandleInteractionErrorAsync;

        client.Ready += async () =>
        {
            await interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), services);

            if (Cfg.GuildMode)
            {
                foreach (var cmd in await client.GetGlobalApplicationCommandsAsync())
                {
                    await cmd.DeleteAsync();
                }

                foreach (var guildId in Cfg.GuildIds)
                {
                    await interactions.RegisterCommandsToGuildAsync(guildId);
                }
            }
            else
            {
                foreach (var guild in await client.Rest.GetGuildsAsync())
                {
                    await guild.DeleteSlashCommandsAsync();
                }

                await interactions.RegisterCommandsGloballyAsync();
            }
        };
        client.InteractionCreated += OnInteractionCreatedAsync;

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        interactions.Dispose();

        return Task.CompletedTask;
    }

    private async Task HandleInteractionErrorAsync(ICommandInfo cmd, IInteractionContext ctx, IResult result)
    {
        if (result is not ExecuteResult exRes || exRes.Exception.StackTrace?.Contains("at Discord.Interactions.CommandInfo`1.ExecuteInternalAsync") == true)
        {
            // For some reason a NullReferenceException containing the above string is always thrown
            return;
        }

        Log.Error(
            "Error executing command {CommandName}: {Error}",
            cmd.Name,
            exRes.Exception.ToString()
        );

        if (result.IsSuccess)
        {
            return;
        }

        var interaction = ctx.Interaction;
        var errorEmbed = new EmbedBuilder()
            .WithTitle(":warning: Error :warning:")
            .WithDescription(
                "An unhandled exception occurred while executing this command.\n"
                + "This error has been reported to the developer and will be fixed as soon as possible."
            ).WithColor(0x000)
            .Build();

        if (interaction.HasResponded)
        {
            await interaction.FollowupAsync(embed: errorEmbed, ephemeral: true);
        }
        else
        {
            await interaction.RespondAsync(embed: errorEmbed, ephemeral: true);
        }

        var owner = await client.Rest.GetUserAsync(Cfg.OwnerDiscordId);
        var dm = owner.CreateDMChannelAsync();

        var msg =
            $":warning: Unhandled error in channel <#{interaction.ChannelId}>\n"
            + $"User: {interaction.User.Mention}\n"
            + $"Guild ID: `{interaction.GuildId}`\n"
            + $"Timestamp: {DateTimeOffset.Now.ToDiscordTimestampLong()}\n"
            + $"Error: \n```{exRes.Exception}```";

        if (msg.Length > CharacterLimit.DiscordMessageDefault)
        {
            msg = msg[..(CharacterLimit.DiscordMessageDefault - 4)] + "…```";
        }

        await dm.Result.SendMessageAsync(msg);
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
