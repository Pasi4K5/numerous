// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Numerous.Bot.Web.Osu.Models;

namespace Numerous.Bot.Web.Osu;

public static class Extensions
{
    public static bool IsRankedMapper(this OsuUser user)
    {
        return user.RankedBeatmapsetCount > 0
               || user.GuestBeatmapsetCount > 0;
    }

    public static bool IsLovedMapper(this OsuUser user)
    {
        return user.LovedBeatmapsetCount > 0;
    }

    public static bool IsUnrankedMapper(this OsuUser user)
    {
        return
            (user.GraveyardBeatmapsetCount > 0 || user.PendingBeatmapsetCount > 0)
            && !user.IsRankedMapper();
    }
}
