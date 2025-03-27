// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Bot.Web.Osu.Models;

[JsonObject(MemberSerialization.OptIn)]
public sealed record ApiForumPost
{
    [JsonProperty("id")]
    public required int Id { get; init; }

    [JsonProperty("forum_id")]
    public int ForumId { get; set; }

    [JsonProperty("user_id")]
    public required int UserId { get; init; }

    [JsonProperty("created_at")]
    public required DateTimeOffset CreatedAt { get; init; }

    [JsonProperty("body")]
    public required PostBody Body { get; init; }

    [JsonObject(MemberSerialization.OptIn)]
    public sealed class PostBody
    {
        [JsonProperty("raw")]
        public required string Raw { get; init; }
    }
}
