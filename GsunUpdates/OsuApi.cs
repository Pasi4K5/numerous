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

    public async Task<JObject> RequestAsync(string endpoint)
    {
        var token = await GetTokenAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to request endpoint.");
        }

        var responseText = await response.Content.ReadAsStringAsync();
        var responseJson = JObject.Parse(responseText);

        return responseJson;
    }

    private async Task<string> GetTokenAsync()
    {
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

        return responseJson["access_token"]?.Value<string>() ?? "";
    }
}
