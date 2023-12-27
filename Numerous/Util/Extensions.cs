﻿namespace Numerous.Util;

public static class Extensions
{
    public static IEnumerable<string> ToDiscordMessageStrings(this string message)
    {
        var remaining = message;
        var messages = new List<string>();

        while (remaining.Length > 0)
        {
            var index = remaining.Length > 2000
                ? remaining[..2000].LastIndexOf('\n')
                : remaining.Length;

            messages.Add(remaining[..index]);
            remaining = remaining[index..];
        }

        return messages;
    }
}