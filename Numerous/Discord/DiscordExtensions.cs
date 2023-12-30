using Discord;
using Discord.WebSocket;

namespace Numerous.Discord;

public static class DiscordExtensions
{
    public static async Task ReplyAsync(this IMessage message, string text)
    {
        await message.Channel.SendMessageAsync(text, messageReference: new(message.Id));
    }

    public static string ToLogString(this IReadOnlyCollection<SocketSlashCommandDataOption> options)
    {
        var s = "";

        foreach (var option in options)
        {
            s += string.IsNullOrEmpty(option.Value?.ToString())
                ? option.Name
                : $"{option.Name}: \"{option.Value}\", ";

            if (option.Options.Count > 0)
            {
                s += ", " + option.Options.ToLogString() + ", ";
            }
        }

        s = $"[{s[..^2]}]";

        return s;
    }
}
