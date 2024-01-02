﻿// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;

namespace Numerous.Discord;

public static class DiscordExtensions
{
    public static async Task ReplyAsync(this IMessage message, string text)
    {
        await message.Channel.SendMessageAsync(text, messageReference: new(message.Id));
    }

    public static string GetLink(this IMessage msg)
    {
        var guildId = msg.Channel is IGuildChannel channel ? channel.Guild.Id.ToString() : "@me";

        return $"https://discord.com/channels/{guildId}/{msg.Channel.Id}/{msg.Id}";
    }

    public static string ToLogString(this IReadOnlyCollection<SocketSlashCommandDataOption> options)
    {
        if (options.Count == 0)
        {
            return "[]";
        }

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
