using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace GsunUpdates;

public sealed class OsuApi
{
    private const string BaseUrl = "https://osu.ppy.sh/api/v2/";
    private const string TokenUrl = "https://osu.ppy.sh/oauth/token";

    private static uint ClientId => Config.Get().OsuClientId;
    private static string ClientSecret => Config.Get().OsuClientSecret;

    private readonly HttpClient _client = new();

    private string _accessToken = "";

    private DateTime _tokenExpiration = DateTime.MinValue;

    public async Task<JToken?> RequestAsync(string endpoint, Dictionary<string, string>? parameters = null)
    {
        parameters ??= new();

        var token = await GetTokenAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        foreach (var (key, value) in parameters)
        {
            request.Headers.Add(key, value);
        }

        try
        {
            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to request endpoint.");
            }

            var responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseText);
            var responseJson = JToken.Parse(responseText);

            return responseJson;
        }
        catch (Exception)
        {
            return null;
        }
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
                ["scope"] = "public"
            })
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
}
