using Discord;
using JetBrains.Annotations;

namespace Numerous.Database.Entities;

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

    public required ulong GuildId { get; init; }

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
}
