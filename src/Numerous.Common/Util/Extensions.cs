// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Numerous.Common.Util;

public static class Extensions
{
    public static void Ignore<T>(this T _)
    {
    }

    public static T With<T>(this T obj, Action<T> action)
    {
        action(obj);

        return obj;
    }

    public static TResult Let<TSource, TResult>(this TSource obj, Func<TSource, TResult> func) =>
        func(obj);

    public static string ToHexString(this byte[] bytes)
    {
        return BitConverter.ToString(bytes).RemoveAll("-").ToLower();
    }

    public static string RemoveAll(this string s, string oldChars)
    {
        foreach (var oldChar in oldChars)
        {
            s = s.Replace(oldChar.ToString(), "");
        }

        return s;
    }
}
