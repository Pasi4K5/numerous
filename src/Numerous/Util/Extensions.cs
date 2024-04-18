// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Newtonsoft.Json;

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

    // ReSharper disable once UnusedMember.Global
    public static async Task<T?> ToObjectAsync<T>(this Task<HttpResponseMessage> response)
    {
        var responseText = await (await response).Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(responseText);
    }

    public static Color Opaque(this Color color)
    {
        return Color.FromArgb(255, color);
    }
}
