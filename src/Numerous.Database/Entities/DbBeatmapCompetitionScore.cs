// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Numerous.Database.Entities;

[Table("beatmap_competition_score")]
[Index(nameof(OnlineId), IsUnique = true)]
public sealed class DbBeatmapCompetitionScore : DbEntity<Guid>
{
    [Column("Md5Hash")]
    public override Guid Id { get; set; }

    public ulong? OnlineId { get; set; }

    public long TotalScore { get; set; }

    [Column(TypeName = "char(2)[]")]
    public string[] Mods { get; set; } = [];

    public double Accuracy { get; set; }
    public int MaxCombo { get; set; }
    public int GreatCount { get; set; }
    public int OkCount { get; set; }
    public int MehCount { get; set; }
    public int MissCount { get; set; }

    public Instant DateTime { get; set; }

    [ForeignKey($"{nameof(GuildId)}, {nameof(StartTime)}")]
    public DbBeatmapCompetition Competition { get; set; } = null!;

    public ulong GuildId { get; set; }
    public Instant StartTime { get; set; }

    public DbOsuUser Player { get; set; } = null!;
    public int PlayerId { get; set; }

    public DbReplay? Replay { get; set; }
}
