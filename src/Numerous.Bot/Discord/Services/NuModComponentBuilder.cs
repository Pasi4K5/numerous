// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;

namespace Numerous.Bot.Discord.Services;

public static class NuModComponentBuilder
{
    internal const string DeleteMessageId = "numod:delete_message";
    internal const string ResolveId = "numod:resolve";

    public static MessageComponent BuildDeleteComponents(
        ulong reportChannelId,
        ulong reportMsg,
        ulong channelId,
        ulong messageId)
    {
        return BuildDeleteComponents(
            reportChannelId: reportChannelId,
            reportMsg: reportMsg,
            channelId: channelId,
            messageId: messageId,
            false
        );
    }

    public static MessageComponent BuildDisabledDeleteComponents()
    {
        return BuildDeleteComponents(null, null, null, null, true);
    }

    private static MessageComponent BuildDeleteComponents(
        ulong? reportChannelId,
        ulong? reportMsg,
        ulong? channelId,
        ulong? messageId,
        bool disabled = false)
    {
        return new ComponentBuilder()
            .WithButton(
                "Delete Message",
                $"{DeleteMessageId}:{reportChannelId},{reportMsg},{channelId},{messageId}",
                ButtonStyle.Danger,
                Emoji.Parse(":wastebasket:"),
                disabled: disabled
            )
            .WithButton(
                "Resolve",
                $"{ResolveId}:{reportChannelId},{reportMsg}",
                ButtonStyle.Success,
                Emoji.Parse(":white_check_mark:"),
                disabled: disabled
            )
            .Build();
    }

    public static Embed BuildWarningEmbed(string desc)
    {
        return new EmbedBuilder()
            .WithColor(Color.Orange)
            .WithTitle("NuMod - Warning")
            .WithDescription(desc)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .Build();
    }
}
