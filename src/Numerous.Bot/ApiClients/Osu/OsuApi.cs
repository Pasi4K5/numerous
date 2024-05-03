// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Numerous.Bot.Configuration;
using Numerous.Bot.DependencyInjection;

namespace Numerous.Bot.ApiClients.Osu;

[SingletonService]
public sealed partial class OsuApi(IConfigService config)
{
    private const string BaseUrl = "https://osu.ppy.sh/api/v2/";
    private const string TokenUrl = "https://osu.ppy.sh/oauth/token";

    private readonly HttpClient _client = new();

    private string _accessToken = "";

    private uint ClientId => config.Get().OsuClientId;
    private string ClientSecret => config.Get().OsuClientSecret;

    private DateTime _tokenExpiration = DateTime.MinValue;

    private async Task<T?> RequestRefAsync<T>(string endpoint, params (string key, string value)[] parameters)
        where T : class?
    {
        var result = new Wrapper<T?>();

        return !await RequestAsync(endpoint, result, parameters) ? null : result.Data;
    }

    // private async Task<T?> RequestValAsync<T>(string endpoint, params (string key, string value)[] parameters)
    //     where T : struct
    // {
    //     var result = new Wrapper<T?>();
    //
    //     return !await RequestAsync(endpoint, result, parameters) ? null : result.Data;
    // }

    // private async Task<ICollection<T>> RequestCollectionAsync<T>(string endpoint, params (string key, string value)[] parameters)
    // {
    //     return await RequestRefAsync<ICollection<T>>(endpoint, parameters) ?? Array.Empty<T>();
    // }

    private async Task<bool> RequestAsync<T>(string endpoint, Wrapper<T?> result, params (string key, string value)[] parameters)
    {
        var token = await GetTokenAsync();

        var uri = BaseUrl + endpoint + "?" + string.Join("&", parameters.Select(p => $"{HttpUtility.UrlEncode(p.key)}={HttpUtility.UrlEncode(p.value)}"));

        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var responseText = await response.Content.ReadAsStringAsync();
        var responseObject = JsonConvert.DeserializeObject<T>(responseText);

        result.Data = responseObject;

        return true;
    }

    private async Task<string> GetTokenAsync()
    {
        if (_tokenExpiration > DateTime.Now)
        {
            return _accessToken;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = ClientId.ToString(),
                ["client_secret"] = ClientSecret,
                ["scope"] = "public",
            }),
        };

        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to get token.");
        }

        var responseText = await response.Content.ReadAsStringAsync();
        var responseJson = JObject.Parse(responseText);

        _tokenExpiration = DateTime.Now + TimeSpan.FromSeconds((responseJson["expires_in"]?.Value<int>() ?? 0) - 10);

        _accessToken = responseJson["access_token"]?.Value<string>() ?? "";

        return _accessToken;
    }

    private sealed record Wrapper<T>
    {
        public T? Data { get; set; }
    }
}
