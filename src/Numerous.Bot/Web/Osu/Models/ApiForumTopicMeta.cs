// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Bot.Web.Osu.Models;

[JsonObject(MemberSerialization.OptIn)]
public sealed record ApiForumTopicMeta
{
    [JsonProperty("id")]
    public required int Id { get; init; }

    [JsonProperty("forum_id")]
    public required int ForumId { get; init; }

    [JsonProperty("user_id")]
    public required int UserId { get; init; }

    [JsonProperty("created_at")]
    public required DateTimeOffset CreatedAt { get; init; }

    [JsonProperty("updated_at")]
    public required DateTimeOffset UpdatedAt { get; init; }

    [JsonProperty("title")]
    public required string Title { get; init; }

    [JsonProperty("first_post_id")]
    public required int FirstPostId { get; init; }
}
