// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using JetBrains.Annotations;

namespace Numerous.Bot.Database.Entities;

public sealed record DiscordMessage : DbEntity<ulong>
{
    [UsedImplicitly]
    public DiscordMessage()
    {
    }

    public DiscordMessage(IMessage message)
    {
        ChannelId = message.Channel.Id;
        ChannelName = message.Channel.Name;
        ThreadId = message.Thread?.Id;
        IsNewThread = message.Thread?.MessageCount == 1;
        Contents = new[] { message.Content };
        CleanContents = new[] { message.CleanContent };
        AuthorId = message.Author.Id;
        AuthorUsername = message.Author.Username;
        Timestamps = new[] { message.CreatedAt };
        ReferenceMessageId = message.Reference?.MessageId.GetValueOrDefault();
    }

    public bool Equals(DiscordMessage? other)
    {
        return other is not null
               && Contents.SequenceEqual(other.Contents)
               && CleanContents.SequenceEqual(other.CleanContents)
               && Timestamps.SequenceEqual(other.Timestamps);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }

    public ulong GuildId { get; init; }

    public ulong ChannelId { get; init; }

    public string ChannelName { get; init; } = null!;

    public ulong? ThreadId { get; init; }

    public bool IsNewThread { get; init; }

    public ICollection<string> Contents { get; init; } = null!;

    public ICollection<string> CleanContents { get; init; } = null!;

    public ulong AuthorId { get; init; }

    public string AuthorUsername { get; init; } = null!;

    public ICollection<DateTimeOffset> Timestamps { get; init; } = Array.Empty<DateTimeOffset>();

    public ulong? ReferenceMessageId { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public bool IsHidden { get; set; }
}
