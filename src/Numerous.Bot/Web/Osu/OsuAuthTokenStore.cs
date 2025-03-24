// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Newtonsoft.Json;
using Numerous.Bot.Web.Osu.Models;
using Numerous.Common.Config;

namespace Numerous.Bot.Web.Osu;

public interface IOsuTokenProvider
{
    Task<string> GetTokenAsync();
}

public sealed class OsuTokenProvider(IConfigProvider cfgProvider, IHttpClientFactory clientFactory) : IOsuTokenProvider
{
    private static readonly TimeSpan _tokenExpirationBuffer = TimeSpan.FromSeconds(10);

    private Token? _token;

    private const string BaseUrl = IOsuApi.BaseUrl;
    private const string TokenUrl = $"{BaseUrl}/oauth/token";

    private readonly HttpClient _client = clientFactory.CreateClient();

    private int ClientId => cfgProvider.Get().OsuClientId;
    private string ClientSecret => cfgProvider.Get().OsuClientSecret;

    public async Task<string> GetTokenAsync()
    {
        if (_token is not null && _token.ExpiresAt - _tokenExpirationBuffer > DateTimeOffset.UtcNow)
        {
            return _token.AccessToken;
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
            throw new HttpRequestException($"Failed to get token: {response.ReasonPhrase}");
        }

        var responseText = await response.Content.ReadAsStringAsync();
        var responseJson = JsonConvert.DeserializeObject<ApiOsuTokenResponse>(responseText);

        if (responseJson is null)
        {
            throw new JsonSerializationException("Failed to deserialize token.");
        }

        _token = new Token
        {
            AccessToken = responseJson.AccessToken,
            ExpiresAt = DateTime.Now + TimeSpan.FromSeconds(responseJson.ExpiresInSeconds),
        };

        return _token.AccessToken;
    }

    private record Token
    {
        public string AccessToken { get; init; } = "";
        public DateTimeOffset ExpiresAt { get; init; }
    }
}
