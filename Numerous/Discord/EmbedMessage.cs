// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Numerous.Util;

namespace Numerous.Discord;

public sealed class EmbedMessage
{
    private const int MaxTitleLength = 256;
    private const int MaxDescriptionLength = 4096;

    public string Title
    {
        set => _builder.WithTitle(value.LimitLength(MaxTitleLength));
    }

    public string Description
    {
        set => _builder.WithDescription(value.LimitLength(MaxDescriptionLength));
    }

    public ResponseType Type
    {
        set => _builder.WithColor(value switch
        {
            ResponseType.Info => Color.Blue,
            ResponseType.Success => Color.Green,
            ResponseType.Warning => Color.Orange,
            ResponseType.Error => Color.DarkRed,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        });
    }

    public Embed Embed => _builder.Build();

    private readonly EmbedBuilder _builder = new();

    public enum ResponseType
    {
        Info,
        Success,
        Warning,
        Error,
    }
}
