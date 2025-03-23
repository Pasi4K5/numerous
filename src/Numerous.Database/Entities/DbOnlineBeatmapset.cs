﻿// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations.Schema;

namespace Numerous.Database.Entities;

[Table("online_beatmapset")]
public sealed class DbOnlineBeatmapset : DbEntity<uint>
{
    public DbOsuUser Creator { get; set; } = null!;
    public uint CreatorId { get; set; }

    public ICollection<DbOnlineBeatmap> Beatmaps { get; set; } = [];
    public ICollection<DbBeatmapsetStats> Stats { get; set; } = [];
}
