// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

global using static Numerous.Bot.Util.GlobalUtil;

namespace Numerous.Bot.Util;

public static class GlobalUtil
{
    public static bool AllAreNull<T>(params IEnumerable<T> p)
    {
        return p.All(x => x is null);
    }
}
