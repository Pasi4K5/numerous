using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace GsunUpdates;

public class OsuApi
{
    private const string BaseUrl = "https://osu.ppy.sh/api/v2/";
    private const string TokenUrl = "https://osu.ppy.sh/oauth/token";

    private uint ClientId => Config.Get().OsuClientId;
    private string ClientSecret => Config.Get().OsuClientSecret;

    private readonly HttpClient _client = new();

    private string _token = "";
    private DateTime _tokenExpiry = DateTime.MinValue;

    public async Task StartAsync()
    {
        await GetTokenAsync();

        var _ = Task.Run(async () =>
        {
            while (true)
            {
                var validFor = _tokenExpiry - DateTime.UtcNow - TimeSpan.FromSeconds(5);

                await Task.Delay(validFor);

                await GetTokenAsync();
            }
        });
    }

    public async Task<JObject> RequestAsync(string endpoint)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to request endpoint.");
        }

        var responseText = await response.Content.ReadAsStringAsync();
        var responseJson = JObject.Parse(responseText);

        return responseJson;
    }

    private async Task GetTokenAsync()
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

        _token = responseJson["access_token"]?.Value<string>() ?? "";
        _tokenExpiry = DateTime.UtcNow + TimeSpan.FromSeconds(responseJson["expires_in"]?.Value<int>() ?? 0);
    }
}
