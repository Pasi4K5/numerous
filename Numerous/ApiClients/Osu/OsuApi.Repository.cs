// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json.Linq;
using Numerous.ApiClients.Osu.Models;

namespace Numerous.ApiClients.Osu;

public partial class OsuApi
{
    // private readonly string[] _modes = { "osu", "taiko", "fruits", "mania" };

    // public async Task<OsuUser?> GetUserAsync(string user, bool prioritizeId = false)
    // {
    //     return await RequestValAsync<OsuUser>($"users/{user}", ("key", prioritizeId ? "id" : "username"));
    // }

    /// <summary>
    /// Prioritizes the user ID over the username.
    /// </summary>
    public async Task<JObject?> GetUserJsonAsync(string user)
    {
        return await RequestRefAsync<JObject>($"users/{user}", ("key", "id"));
    }

    public async Task<Beatmapset?> GetBeatmapsetAsync(uint id)
    {
        return await RequestValAsync<Beatmapset>($"beatmapsets/{id}");
    }

    // public async IAsyncEnumerable<OsuScore> GetRecentScoresAsync(uint userId)
    // {
    //     var tasks = _modes.Select(m => RequestCollectionAsync<OsuScore>($"users/{userId}/scores/recent", ("mode", m)));
    //
    //     foreach (var task in tasks)
    //     {
    //         var results = await task;
    //
    //         foreach (var result in results)
    //         {
    //             yield return result;
    //         }
    //     }
    // }

    public async Task<JObject?> WikiLookupAsync(string query)
    {
        var result = await RequestRefAsync<JObject>("search", ("mode", "wiki_page"), ("query", query));

        return result is null
            ? null
            : new JObject
            {
                ["pages"] = new JArray(result["wiki_page"]!.ToObject<JObject>()!["data"]!.ToObject<JArray>()!.Select(page => new JObject
                {
                    ["path"] = page["path"],
                    ["tags"] = page["tags"],
                })),
            };
    }

    public async Task<JObject?> UserLookupAsync(string query)
    {
        var result = await RequestRefAsync<JObject>("search", ("mode", "user"), ("query", query));

        return result is null
            ? null
            : new JObject
            {
                ["users"] = new JArray(result["user"]!.ToObject<JObject>()!["data"]!.ToObject<JArray>()!.Select(user => new JObject
                {
                    ["id"] = user["id"],
                    ["username"] = user["username"],
                })),
            };
    }

    public async Task<string?> GetWikiPage(string path)
    {
        return (await RequestRefAsync<JObject>($"wiki/en/{path}"))?["markdown"]?.Value<string>();
    }
}
