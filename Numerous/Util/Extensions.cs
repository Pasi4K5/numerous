// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;

namespace Numerous.Util;

public static class Extensions
{
    public static IEnumerable<(TSource element, int index)> WithIndexes<TSource>(this IEnumerable<TSource> enumerable)
    {
        var i = 0;

        foreach (var item in enumerable)
        {
            yield return (item, i++);
        }
    }

    public static string OnlyIf(this string s, bool condition)
    {
        return condition ? s : "";
    }

    public static string LimitLength(this string s, int maxLength)
    {
        return s.Length > maxLength ? s[..(maxLength - 1)] + "…" : s;
    }

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
