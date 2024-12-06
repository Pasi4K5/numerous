// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Numerous.Database.Entities;

[Table("guild_stats_entry")]
[PrimaryKey(nameof(GuildId), nameof(Timestamp))]
[Index(nameof(GuildId))]
public sealed class DbGuildStatsEntry
{
    public DbGuild Guild { get; set; } = null!;
    public ulong GuildId { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public int MemberCount { get; set; }
}
