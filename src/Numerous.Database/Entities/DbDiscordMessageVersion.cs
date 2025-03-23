// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Numerous.Common.Util;

namespace Numerous.Database.Entities;

[Table("discord_message_version")]
[PrimaryKey(nameof(MessageId), nameof(Timestamp))]
public sealed class DbDiscordMessageVersion
{
    [MaxLength(CharacterLimit.DiscordMessageNitro)]
    public required string RawContent { get; set; }

    [MaxLength(CharacterLimit.DiscordMessageNitro)]
    [Comment("If NULL, the clean content is the same as the raw content.")]
    public string? CleanContent { get; set; }

    public required Instant Timestamp { get; set; }

    public DbDiscordMessage Message { get; set; } = null!;
    public ulong MessageId { get; set; }
}
