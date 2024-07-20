// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Numerous.Bot.Web.Osu.Models;
using Refit;

namespace Numerous.Bot.Web.Osu;

public interface IOsuApi
{
    internal const string BaseUrl = "https://osu.ppy.sh";

    private const string Api = "/api/v2";

    /// <summary>
    /// https://osu.ppy.sh/docs/index.html#get-user
    /// </summary>
    [Get(Api + "/users/{user}")]
    Task<OsuUserExtended> GetUserAsync(string user, [Query] string? key = null);

    /// <summary>
    /// https://osu.ppy.sh/docs/index.html#get-user-beatmaps
    /// </summary>
    [Get(Api + "/users/{user}/beatmapsets/{type}")]
    Task<BeatmapsetExtended[]> GetUserBeatmapsetsAsync(uint user, string type, [Query] string limit);
}
