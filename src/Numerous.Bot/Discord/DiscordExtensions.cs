// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;

namespace Numerous.Bot.Discord;

public static partial class DiscordExtensions
{
    [GeneratedRegex(@"discord\.com/channels/\d+/(\d+)/(\d+)")]
    private static partial Regex MessageLinkRegex();

    public static async Task ReplyAsync(this IMessage message, string text)
    {
        await message.Channel.SendMessageAsync(text, messageReference: new(message.Id));
    }

    public static string GetLink(this IMessage msg)
    {
        var guildId = msg.Channel is IGuildChannel channel ? channel.Guild.Id.ToString() : "@me";

        return $"https://discord.com/channels/{guildId}/{msg.Channel.Id}/{msg.Id}";
    }

    public static ulong? ParseMessageId(this string linkOrId)
    {
        if (ulong.TryParse(linkOrId, out var id))
        {
            return id;
        }

        var match = MessageLinkRegex().Match(linkOrId);

        return match.Success
            ? ulong.Parse(match.Groups[2].Value)
            : null;
    }

    public static string ToLogString(this IReadOnlyCollection<SocketSlashCommandDataOption> options)
    {
        if (options.Count == 0)
        {
            return "[]";
        }

        var sb = new StringBuilder();

        foreach (var option in options)
        {
            sb.Append(
                string.IsNullOrEmpty(option.Value?.ToString())
                    ? option.Name
                    : $"{option.Name}: \"{option.Value}\", "
            );

            if (option.Options.Count > 0)
            {
                sb.Append(", " + option.Options.ToLogString() + ", ");
            }
        }

        return $"[{sb.ToString()[..^2]}]";
    }

    public static string ToDiscordTimestampRel(this DateTimeOffset timestamp)
    {
        return $"<t:{timestamp.ToUnixTimeSeconds()}:R>";
    }

    public static string ToDiscordTimestampDateTime(this DateTimeOffset timestamp)
    {
        return $"<t:{timestamp.ToUnixTimeSeconds()}:f>";
    }

    public static string ToDiscordTimestampLong(this DateTimeOffset timestamp)
    {
        return $"<t:{timestamp.ToUnixTimeSeconds()}:D> <t:{timestamp.ToUnixTimeSeconds()}:T>";
    }

    public static string WithLink(this string s, string url)
    {
        return $"[{s}]({url})";
    }

    public static string Mention(this IMessageChannel channel)
    {
        return MentionUtils.MentionChannel(channel.Id);
    }
}
