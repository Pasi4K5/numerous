﻿using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace GsunUpdates;

public sealed class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly JsonDb _db;

    public CommandHandler(DiscordSocketClient client, JsonDb db)
    {
        _client = client;
        _db = db;

        _client.Ready += CreateCommand;
        _client.SlashCommandExecuted += HandleSlashCommand;
    }

    private async Task CreateCommand()
    {
        var command = new SlashCommandBuilder()
            .WithName("setchannel")
            .WithDescription("Sets the channel to send Gsun updates to.")
            .WithDMPermission(false)
            .WithDefaultPermission(false)
            .WithDefaultMemberPermissions(GuildPermission.Administrator)
            .AddOption("channel", ApplicationCommandOptionType.Channel, "The channel to send Gsun updates to.", true)
            .Build();

        try
        {
            await _client.CreateGlobalApplicationCommandAsync(command);
        }
        catch (HttpException e)
        {
            Console.WriteLine(string.Join('\n', e.Errors));
        }
    }

    private async Task HandleSlashCommand(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "setchannel":
                if (command.Data.Options.FirstOrDefault()?.Value is not SocketGuildChannel channel)
                {
                    await command.RespondAsync("No valid channel was provided.");

                    return;
                }

                var guildId = command.GuildId.ToString();

                if (guildId is not null)
                {
                    var data = _db.Data;
                    var channels = JArray.FromObject(_db.Data["channels"] ?? new JArray());
                    channels.Add(channel.Id);
                    data["channels"] = channels;
                    _db.Save(data);
                }

                await command.RespondAsync($"Set Gsun update channel to {channel.Mention()}.");

                break;
        }
    }
}
