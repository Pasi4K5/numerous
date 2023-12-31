using System.Drawing;

namespace Numerous.Util;

public static class Extensions
{
    public static string RemoveAll(this string s, string oldChars)
    {
        foreach (var oldChar in oldChars)
        {
            s = s.Replace(oldChar.ToString(), "");
        }

        return s;
    }

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

    public static Color Opaque(this Color color)
    {
        return Color.FromArgb(255, color);
    }
}
