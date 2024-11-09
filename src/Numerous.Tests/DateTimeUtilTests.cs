// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Numerous.Bot.Util;

namespace Numerous.Tests;

public sealed class DateTimeUtilTests
{
    private static readonly (
        DateTime now,
        ushort? year,
        byte? month,
        byte? day,
        byte? hour,
        byte? minute,
        byte? second,
        DateTime expected
        )[] _parametersToDateTimeTestCases =
        [
            (
                new(2000, 2, 2, 12, 10, 20),
                2010, null, null, null, null, null,
                new(2010, 1, 1, 0, 0, 0)
            ),
            (
                new(2000, 2, 2, 12, 10, 20),
                null, null, null, null, null, 30,
                new(2000, 2, 2, 12, 10, 30)
            ),
            (
                new(2000, 2, 2, 0, 10, 20),
                null, null, null, 12, null, null,
                new(2000, 2, 2, 12, 0, 0)
            ),
            (
                new(2000, 2, 2, 0, 10, 20),
                null, null, null, 12, 20, null,
                new(2000, 2, 2, 12, 20, 0)
            ),
            (
                new(2000, 2, 2, 12, 10, 20),
                null, null, null, 10, null, null,
                new(2000, 2, 3, 10, 0, 0)
            ),
            (
                new(2000, 2, 2, 12, 10, 20),
                null, null, null, 10, null, null,
                new(2000, 2, 3, 10, 0, 0)
            ),
            (
                new(2000, 2, 2, 12, 10, 20),
                null, null, null, 12, null, null,
                new(2000, 2, 3, 12, 0, 0)
            ),
            (
                new(2000, 2, 2, 12, 10, 20),
                null, 1, null, null, null, null,
                new(2001, 1, 1, 0, 0, 0)
            ),
            (
                new(2000, 2, 2, 12, 10, 20),
                null, null, null, 12, 5, null,
                new(2000, 2, 3, 12, 5, 0)
            ),
            (
                new(2000, 12, 31, 23, 59, 50),
                null, null, null, null, null, 10,
                new(2001, 1, 1, 0, 0, 10)
            ),
            (
                new(2000, 2, 2, 12, 10, 20),
                null, 2, null, null, null, null,
                new(2001, 2, 1, 0, 0, 0)
            ),
        ];

    public static IEnumerable<object[]> ParametersToDateData =>
        _parametersToDateTimeTestCases.Select(x => new object[] { x.now, x.year, x.month, x.day, x.hour, x.minute, x.second, x.expected });

    [Theory]
    [MemberData(nameof(ParametersToDateData))]
    public void ParametersToDateTime_ValidParameters_ReturnsCorrectDateTime(
        DateTime now,
        ushort? year,
        byte? month,
        byte? day,
        byte? hour,
        byte? minute,
        byte? second,
        DateTime expected
    )
    {
        var result = DateTimeUtil.ParametersToDateTime(now, year, month, day, hour, minute, second);

        result.Should().Be(expected);
    }

    [Fact]
    public void ParamtersToDateTime_AllNullParameters_ThrowsArgumentNullException()
    {
        Action act = () => DateTimeUtil.ParametersToDateTime(
            new(2000, 2, 2, 12, 10, 20),
            null, null, null, null, null, null
        );

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ParamtersToDateTime_PastDate_ThrowsArgumentOutOfRangeException()
    {
        Action act = () => DateTimeUtil.ParametersToDateTime(
            new(2000, 2, 2, 12, 10, 20),
            1999, null, null, null, null, null
        );

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
