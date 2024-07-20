// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics;
using System.Net;
using Numerous.Bot.Web.Osu.Models;
using Numerous.Common.DependencyInjection;
using Refit;

namespace Numerous.Bot.Web.Osu;

public interface IOsuApiRepository
{
    Task<OsuUserExtended> GetUserAsync(string query, bool prioritizeUsername = false);
    Task<OsuUserExtended> GetUserByIdAsync(uint userId);
    Task<OsuUserExtended> GetUserByUsernameAsync(string username);
    Task<BeatmapsetExtended[]> GetUserBeatmapsetsAsync(uint userId, BeatmapType type);
}

[SingletonService<IOsuApiRepository>]
public sealed class OsuApiRepository(IOsuApi api) : IOsuApiRepository
{
    public async Task<OsuUserExtended> GetUserAsync(string query, bool prioritizeUsername = false)
    {
        if (prioritizeUsername)
        {
            try
            {
                return await GetUserByUsernameAsync(query);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
            }
        }

        return await api.GetUserAsync(query);
    }

    public async Task<OsuUserExtended> GetUserByIdAsync(uint userId)
    {
        return await api.GetUserAsync(userId.ToString(), "id");
    }

    public async Task<OsuUserExtended> GetUserByUsernameAsync(string username)
    {
        return await api.GetUserAsync(username, "username");
    }

    public async Task<BeatmapsetExtended[]> GetUserBeatmapsetsAsync(uint userId, BeatmapType type)
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
            BeatmapType.MostPlayed => throw new NotSupportedException($"{nameof(BeatmapType.MostPlayed)} is not supported"),
            _ => throw new UnreachableException(),
        };

        return await api.GetUserBeatmapsetsAsync(userId, typeStr, "10000");
    }
}
