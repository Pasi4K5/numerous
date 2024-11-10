// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Numerous.Database.Dtos;

/// <remarks>At least one of <see cref="Title"/> or <see cref="Description"/> must be set.</remarks>
public sealed class JoinMessageDto : IdDto<ulong>
{
    /// <summary>
    /// Equivalent to <see cref="GuildId"/>.
    /// </summary>
    public override ulong Id { get => GuildId; set => GuildId = value; }

    public required ulong GuildId { get; set; }
    public required ulong ChannelId { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }
}
