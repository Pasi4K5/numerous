// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Frozen;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;

namespace Numerous.Database.Util;

public static class OsuUtil
{
    private static readonly FrozenDictionary<string, Mod> _acronymModDict;

    static OsuUtil()
    {
        var types = typeof(OsuModAutoplay).Assembly.ExportedTypes.Where(t => t.IsSubclassOf(typeof(Mod)) && !t.IsAbstract);
        _acronymModDict = types.Select(t => (Mod?)Activator.CreateInstance(t)).OfType<Mod>().ToFrozenDictionary(m => m.Acronym);
    }

    public static IEnumerable<Mod> ToModArray(this IEnumerable<string> acronyms)
    {
        return acronyms.Select(GetModByAcronym).ToArray();
    }

    private static Mod GetModByAcronym(string acronym)
    {
        return _acronymModDict[acronym];
    }
}
