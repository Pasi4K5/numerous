// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;

namespace Numerous.Bot.Web.SauceNao;

public readonly record struct SauceNaoResponse
{
    [JsonProperty("results")]
    public SauceNaoResult[] Results { get; init; }
}

public readonly record struct SauceNaoResult
{
    [JsonProperty("header")]
    public SauceNaoResultHeader Header { get; init; }

    [JsonProperty("data")]
    public SauceNaoResultData Data { get; init; }
}

public readonly record struct SauceNaoResultHeader
{
    [JsonProperty("similarity")]
    public string Similarity { get; init; }

    [JsonProperty("thumbnail")]
    public string Thumbnail { get; init; }

    [JsonProperty("hidden")]
    private int JHidden { get; init; }

    public bool Hidden => JHidden != 0;
}

public readonly record struct SauceNaoResultData
{
    [JsonProperty("ext_urls")]
    private List<string>? JExtUrls { get; init; }

    public List<string> ExtUrls => JExtUrls ?? [];

    [JsonProperty("title")]
    public string? Title { get; init; }

    [JsonProperty("member_name")]
    public string? MemberName { get; init; }
}
