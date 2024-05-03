// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;

namespace Numerous.Bot.Discord.Commands;

public abstract class CommandModule : InteractionModuleBase<SocketInteractionContext>
{
    private static Color GetTypeColor(ResponseType type)
    {
        return type switch
        {
            ResponseType.Info => Color.Blue,
            ResponseType.Success => Color.Green,
            ResponseType.Warning => Color.Orange,
            ResponseType.Error => Color.DarkRed,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    protected async Task RespondWithEmbedAsync(string title = "", string message = "", ResponseType type = ResponseType.Info)
    {
        await RespondAsync(embed: new EmbedBuilder()
            .WithColor(GetTypeColor(type))
            .WithTitle(title)
            .WithDescription(message)
            .Build()
        );
    }

    protected async Task FollowupWithEmbedAsync(string title = "", string message = "", ResponseType type = ResponseType.Info)
    {
        await FollowupAsync(embed: new EmbedBuilder()
            .WithTitle(title)
            .WithColor(GetTypeColor(type))
            .WithDescription(message)
            .Build()
        );
    }

    protected enum ResponseType
    {
        Info,
        Success,
        Warning,
        Error,
    }
}
