// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Numerous.Bot.Web.Osu.Models;

namespace Numerous.Bot.Web.Osu;

public partial class OsuApi
{
    /// <summary>
    /// Prioritizes the user ID over the username.
    /// </summary>
    public async Task<OsuUserExtended?> GetUserAsync(string user)
    {
        return await RequestRefAsync<OsuUserExtended>($"users/{user}");
    }

    public async Task<BeatmapsetExtended[]?> GetUserBeatmapsetsAsync(ulong userId, BeatmapType type)
    {
        var typeStr = type switch
        {
            BeatmapType.Favourite => "favourite",
            BeatmapType.Graveyard => "graveyard",
            BeatmapType.Guest => "guest",
            BeatmapType.Loved => "loved",
            BeatmapType.Nominated => "nominated",
            BeatmapType.Pending => "pending",
            BeatmapType.Ranked => "ranked",
            BeatmapType.MostPlayed => throw new NotSupportedException("MostPlayed is not supported"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

        return await RequestRefAsync<BeatmapsetExtended[]>($"users/{userId}/beatmapsets/{typeStr}", ("limit", "10000"));
    }
}
