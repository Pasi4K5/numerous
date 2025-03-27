// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using osu.Game.Beatmaps;

namespace Numerous.Database.Entities;

[Table("beatmapset_stats")]
[PrimaryKey(nameof(BeatmapsetId), nameof(Timestamp), nameof(UserId))]
public sealed class DbBeatmapsetStats
{
    public DbOnlineBeatmapset Beatmapset { get; set; } = null!;
    public int BeatmapsetId { get; set; }

    public Instant Timestamp { get; set; }

    public BeatmapOnlineStatus Status { get; set; }
    public int PlayCount { get; set; }
    public int FavouriteCount { get; set; }

    public DbOsuUser User { get; set; } = null!;
    public int UserId { get; set; }
}
