// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Numerous.Bot.Discord.Services;
using Numerous.Common.Config;
using Numerous.Web.Auth;

namespace Numerous.Web.Controllers;

[Route("[action]")]
public sealed class AuthController(IConfigProvider cfgProvider) : ControllerBase
{
    private static readonly Dictionary<ulong, string> _states = new();

    private Config Config => cfgProvider.Get();

    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, DiscordConstants.AuthenticationScheme);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Connect()
    {
        const string authorizationUrl = "https://osu.ppy.sh/oauth/authorize";
        const string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$-._~";

        if (HttpContext.User.GetUserId() is not { } userId)
        {
            return Unauthorized();
        }

        var state = RandomNumberGenerator.GetString(charset, 128);
        _states[userId] = state;
        Task.Delay(TimeSpan.FromMinutes(1)).ContinueWith(_ => _states.Remove(userId));

        var redirectUriEnc = HttpUtility.UrlEncode($"{Config.BaseUrl}/redirect/osu");
        var url = $"{authorizationUrl}?client_id={Config.OsuClientId}&redirect_uri={redirectUriEnc}&response_type=code&scope=identify&state={state}";

        return Redirect(url);
    }

    [HttpGet("/redirect/osu")]
    [Authorize]
    public async Task<IActionResult> OsuRedirect(
        string code,
        string state,
        [FromServices] IHttpClientFactory clientFactory,
        [FromServices] DiscordSocketClient discordClient,
        [FromServices] OsuVerifier verifier,
        [FromServices] IHttpContextAccessor httpContextAccessor
    )
    {
        const string tokenUrl = "https://osu.ppy.sh/oauth/token";
        const string profileUrl = "https://osu.ppy.sh/api/v2/me";

        var userId = httpContextAccessor.HttpContext?.User.GetUserId();

        if (userId is null || !_states.TryGetValue(userId.Value, out var expectedState) || state != expectedState)
        {
            return Unauthorized();
        }

        var client = clientFactory.CreateClient();
        var clientId = Config.OsuClientId;
        var clientSecret = Config.OsuClientSecret;

        var msg = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
        msg.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId.ToString(),
            ["client_secret"] = clientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = $"{Config.BaseUrl}/redirect/osu",
        });
        var response = await client.SendAsync(msg);
        var tokenResponse = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync())!;

        var accessToken = tokenResponse["access_token"]?.Value<string>();

        msg = new HttpRequestMessage(HttpMethod.Get, profileUrl);
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        response = await client.SendAsync(msg);
        var profileData = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync())!;
        var osuId = profileData["id"]?.Value<uint>();

        var discordUser = await discordClient.GetUserAsync(userId.Value);

        if (osuId is null || discordUser is null)
        {
            return Unauthorized();
        }

        await verifier.VerifyAsync(discordUser, osuId.Value);

        return Redirect("/");
    }
}
