using Discord;

namespace Numerous.Discord;

public static class DiscordExtensions
{
    public static async Task ReplyAsync(this IMessage message, string text)
    {
        await message.Channel.SendMessageAsync(text, messageReference: new(message.Id));
    }
}
