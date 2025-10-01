// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using Discord.Net;

namespace Numerous.Bot.Discord.Interactions;

public abstract class InteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    protected override async Task RespondAsync(
        string? text = null,
        Embed[]? embeds = null,
        bool isTts = false,
        bool ephemeral = false,
        AllowedMentions? allowedMentions = null,
        RequestOptions? options = null,
        MessageComponent? components = null,
        Embed? embed = null,
        PollProperties? poll = null,
        MessageFlags flags = MessageFlags.None)
    {
        try
        {
            await base.RespondAsync(text, embeds, isTts, ephemeral, allowedMentions, options, components, embed, poll, flags);
        }
        catch (HttpException e) when (e.DiscordCode == DiscordErrorCode.CannotSendEmptyMessage)
        {
        }
    }

    protected static Color GetTypeColor(ResponseType type)
    {
        return type switch
        {
            ResponseType.Info => Color.Blue,
            ResponseType.Success => Color.Green,
            ResponseType.Special => Color.Gold,
            ResponseType.Warning => Color.Orange,
            ResponseType.Error => Color.DarkRed,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    protected async Task RespondWithEmbedAsync(string title = "", string message = "", ResponseType type = ResponseType.Info, bool ephemeral = false)
    {
        await RespondAsync(
            embed: CreateEmbed(title, message, type).Build(),
            ephemeral: ephemeral
        );
    }

    protected async Task FollowupWithEmbedAsync(string title = "", string message = "", ResponseType type = ResponseType.Info)
    {
        await FollowupAsync(embed: CreateEmbed(title, message, type).Build());
    }

    protected EmbedBuilder CreateEmbed(string title = "", string message = "", ResponseType type = ResponseType.Info)
    {
        return new EmbedBuilder()
            .WithTitle(title)
            .WithColor(GetTypeColor(type))
            .WithDescription(message);
    }

    protected enum ResponseType
    {
        Info,
        Success,
        Special,
        Warning,
        Error,
    }
}
