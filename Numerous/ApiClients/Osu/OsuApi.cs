using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Numerous.Configuration;
using Numerous.DependencyInjection;

namespace Numerous.ApiClients.Osu;

[SingletonService]
public sealed partial class OsuApi(ConfigManager config)
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

    private async Task<T?> RequestValAsync<T>(string endpoint, params (string key, string value)[] parameters)
        where T : struct
    {
        var result = new Wrapper<T?>();

        return !await RequestAsync(endpoint, result, parameters) ? null : result.Data;
    }

    private async Task<ICollection<T>> RequestCollectionAsync<T>(string endpoint, params (string key, string value)[] parameters)
    {
        return await RequestRefAsync<ICollection<T>>(endpoint, parameters) ?? Array.Empty<T>();
    }

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
