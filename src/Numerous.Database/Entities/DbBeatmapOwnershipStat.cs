// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Numerous.Database.Entities;

[Table("beatmap_ownership_stats")]
[PrimaryKey(nameof(OwnerId), nameof(BeatmapId), nameof(UserId), nameof(Timestamp))]
public sealed class DbBeatmapOwnershipStat
{
    public DbOsuUser Owner { get; set; } = null!;
    public int OwnerId { get; set; }

    [ForeignKey($"{nameof(BeatmapId)},{nameof(Timestamp)},{nameof(UserId)}")]
    public DbBeatmapStats Stats { get; set; } = null!;

    public int BeatmapId { get; set; }
    public Instant Timestamp { get; set; }
    public int UserId { get; set; }
}
