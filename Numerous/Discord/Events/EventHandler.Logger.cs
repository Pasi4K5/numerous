using Discord.WebSocket;
using Numerous.Util;
using Serilog;

namespace Numerous.Discord.Events;

public partial class DiscordEventHandler
{
    [Init]
    private void Logger_Init()
    {
        _client.SlashCommandExecuted += LogSlashCommand;
    }

    private async Task LogSlashCommand(SocketSlashCommand cmd)
    {
        var guild = cmd.GuildId is not null
            ? await _client.Rest.GetGuildAsync(cmd.GuildId.Value)
            : null;

        Log.Information(
            """User "{User}" (ID: {Uid}) executed slash command "{Cmd}" with args {Args} in channel "{Channel}" (ID: {ChannelId}) in guild "{Guild}" (ID: {GuildId})""",
            cmd.User.Username,
            cmd.User.Id,
            cmd.Data.Name,
            cmd.Data.Options.ToLogString(),
            cmd.Channel,
            cmd.ChannelId,
            guild?.Name ?? "null",
            cmd.GuildId);
    }
}
