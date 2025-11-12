// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Numerous.Database.Entities;

[Table("forum_topic_subscription")]
[PrimaryKey(nameof(ForumTopicId), nameof(ChannelId))]
public sealed class DbForumTopicSubscription
{
    public int ForumTopicId { get; set; }

    public DbMessageChannel Channel { get; set; } = null!;
    public ulong ChannelId { get; set; }
}
