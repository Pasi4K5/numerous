// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Numerous.Database.Entities;

[Table("Guild")]
[Index(nameof(UnverifiedRoleId), IsUnique = true)]
public sealed class DbGuild : DbEntity<ulong>
{
    [DefaultValue(false)]
    public bool TrackMessages { get; set; }

    public DbJoinMessage? JoinMessage { get; set; }
    public ulong? UnverifiedRoleId { get; set; }

    public ICollection<DbChannel> Channels { get; set; } = [];
    public ICollection<DbGroupRoleMapping> GroupRoleMappings { get; set; } = [];
    public ICollection<DbBeatmapCompetition> BeatmapCompetitions { get; set; } = [];
}
