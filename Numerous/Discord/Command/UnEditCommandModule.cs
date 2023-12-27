using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using JetBrains.Annotations;
using MongoDB.Driver;
using Numerous.Database;

namespace Numerous.Discord.Command;

public sealed class UnEditCommandModule(DbManager db, DiscordSocketClient client) : CommandModule
{
    [UsedImplicitly]
    [SlashCommand("unedit", "Reveals all versions of the given message.")]
    public async Task UnEdit(
        [Summary("message", "The message to reveal. Can be a message ID or a link.")]
        string message
    )
    {
        await DeferAsync();

        ulong? messageId = message switch
        {
            _ when ulong.TryParse(message, out var id) => id,
            _ when message.StartsWith("https://discord.com/channels/", StringComparison.Ordinal) => ulong.Parse(
                message.Split('/').Last()),
            _ => null,
        };

        if (messageId is null)
        {
            await FollowupAsync("Invalid message ID or link.");

            return;
        }

        var discordMessage = await db.DiscordMessages.Find(m => m.Id == messageId).FirstOrDefaultAsync();

        if (discordMessage is null)
        {
            await FollowupAsync("Message not found.");

            return;
        }

        var user = await client.Rest.GetUserAsync(discordMessage.AuthorId);

        var embed = new EmbedBuilder()
            .WithAuthor(user.Username, user.GetAvatarUrl())
            .WithDescription($"**User ID:** {discordMessage.AuthorId}")
            .WithColor(new(0xff0000))
            .WithFields(
                discordMessage.Contents.Zip(discordMessage.Timestamps)
                    .Select((x, i) => new EmbedFieldBuilder()
                        .WithName($"Version {i + 1}")
                        .WithValue(x.First + $"\n<t:{x.Second.ToUnixTimeSeconds()}:R>"))
            )
            .Build();

        await FollowupAsync(embed: embed);
    }
}
