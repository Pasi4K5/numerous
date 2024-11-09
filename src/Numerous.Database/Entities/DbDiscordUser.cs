// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Numerous.Database.Entities;

[Table("discord_user")]
public sealed class DbDiscordUser : DbEntity<ulong>
{
    [MaxLength(64)]
    public string? TimeZoneId { get; set; }

    public DbOsuUser OsuUser { get; set; } = null!;

    public ICollection<DbDiscordMessage> Messages { get; set; } = [];
}
