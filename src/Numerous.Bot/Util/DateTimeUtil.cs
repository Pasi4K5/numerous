// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Security.Cryptography;

namespace Numerous.Bot.Util;

public static class DateTimeUtil
{
    private static readonly ushort[] _dateTimeDefaults = [default, 1, 1, 0, 0, 0];

    /// <exception cref="ArgumentOutOfRangeException">The given date is not in the future.</exception>
    /// <exception cref="ArgumentNullException">All the given parameters are <see langword="null"/>.</exception>
    public static DateTime ParametersToDateTime(
        DateTime now,
        ushort? year,
        byte? month,
        byte? day,
        byte? hour,
        byte? minute,
        byte? second
    )
    {
        Span<int?> values = [year, month, day, hour, minute, second];

        var firstParamIdx = 0;

        Span<int> nowArr =
        [
            now.Year,
            now.Month,
            now.Day,
            now.Hour,
            now.Minute,
            now.Second,
        ];

        while (firstParamIdx < values.Length && values[firstParamIdx] is null)
        {
            firstParamIdx++;
        }

        if (firstParamIdx >= values.Length)
        {
            throw new ArgumentNullException("All of the given parameters are null.", innerException: null);
        }

        for (var i = firstParamIdx + 1; i < values.Length; i++)
        {
            values[i] ??= _dateTimeDefaults[i];
        }

        for (var i = 0; i < firstParamIdx; i++)
        {
            values[i] = nowArr[i];
        }

        var date = SpanToDateTime(values);

        for (var i = firstParamIdx - 1; i >= 0; i--)
        {
            if (date > now)
            {
                break;
            }

            Increment(ref values, i);

            if (i < firstParamIdx - 1)
            {
                Increment(ref values, i + 1, -1);
            }

            date = SpanToDateTime(values);
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(now, date);

        return date;

        static DateTime SpanToDateTime(ReadOnlySpan<int?> values)
        {
            var year = values[0] ?? default;
            var month = values[1] ?? 1;
            var day = values[2] ?? 1;
            var hour = values[3] ?? 0;
            var minute = values[4] ?? 0;
            var second = values[5] ?? 0;

            return new DateTime(year, month, day, hour, minute, second);
        }

        static void Increment(ref Span<int?> dateTimeArray, int index, int value = 1)
        {
            var dateTime = SpanToDateTime(dateTimeArray);

            var dt = dateTime;
            Func<int, DateTime> addFunc = index switch
            {
                0 => dateTime.AddYears,
                1 => dateTime.AddMonths,
                2 => x => dt.AddDays(x),
                3 => x => dt.AddHours(x),
                4 => x => dt.AddMinutes(x),
                5 => x => dt.AddSeconds(x),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };

            dateTime = addFunc(value);

            dateTimeArray[0] = dateTime.Year;
            dateTimeArray[1] = dateTime.Month;
            dateTimeArray[2] = dateTime.Day;
            dateTimeArray[3] = dateTime.Hour;
            dateTimeArray[4] = dateTime.Minute;
            dateTimeArray[5] = dateTime.Second;
        }
    }

    public static DateTimeOffset TimeOfDayFromUserId(int id)
    {
        var hash = MD5.HashData(BitConverter.GetBytes(id));
        var intHash = BitConverter.ToUInt128(hash.AsSpan()[..(128 / 8)]);
        var totalMinutes = (int)(intHash % (60 * 24));

        return DateTimeOffset.MinValue.Date.AddDays(1).AddMinutes(totalMinutes);
    }
}
