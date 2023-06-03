using Discord;
using Discord.WebSocket;

namespace GsunUpdates;

public static class Extensions
{
    public static string Mention(this SocketGuildChannel channel) =>
        MentionUtils.MentionChannel(channel.Id);

    public static IEnumerable<string> ToDiscordMessageStrings(this string message)
    {
        var remaining = message.Replace("*", "\\*");
        var messages = new List<string>();

        while (remaining.Length > 0)
        {
            var index = remaining.Length > 2000
                ? remaining.Substring(0, 2000).LastIndexOf('\n')
                : remaining.Length;

            messages.Add(remaining.Substring(0, index));
            remaining = remaining.Substring(index);
        }

        return messages;
    }
}
