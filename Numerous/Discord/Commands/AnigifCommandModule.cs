// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.Interactions;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Numerous.Configuration;
using Numerous.Util;

namespace Numerous.Discord.Commands;

[Group("anigif", "Sends an anime GIF.")]
public sealed class AnigifCommandModule(ConfigManager cm, HttpClient client) : CommandModule
{
    private const string BaseUrl = "https://waifu.it/api/v4/";

    [SlashCommand(
        "communication",
        "Sends an anime GIF (communication)."
    )]
    [UsedImplicitly]
    public async Task CommunicationCommand(
        [Choice("bye", "bye")]
        [Choice("hi", "hi")]
        [Choice("nope", "nope")]
        [Choice("thumbsup", "thumbsup")]
        [Choice("wave", "wave")]
        [Choice("yes", "yes")]
        string interaction
    )
    {
        await ExecuteAsync(interaction);
    }

    [SlashCommand(
        "wholesome",
        "Sends an anime GIF (wholesome)."
    )]
    [UsedImplicitly]
    public async Task WholesomeCommand(
        [Choice("baka", "baka")]
        [Choice("blush", "blush")]
        [Choice("cheer", "cheer")]
        [Choice("cuddle", "cuddle")]
        [Choice("feed", "feed")]
        [Choice("glomp", "glomp")]
        [Choice("happy", "happy")]
        [Choice("highfive", "highfive")]
        [Choice("hold", "hold")]
        [Choice("hug", "hug")]
        [Choice("kiss", "kiss")]
        [Choice("lick", "lick")]
        [Choice("love", "love")]
        [Choice("nom", "nom")]
        [Choice("nuzzle", "nuzzle")]
        [Choice("pat", "pat")]
        [Choice("peck", "peck")]
        [Choice("poke", "poke")]
        [Choice("pout", "pout")]
        [Choice("smile", "smile")]
        [Choice("tease", "tease")]
        [Choice("tickle", "tickle")]
        [Choice("wag", "wag")]
        [Choice("wink", "wink")]
        string interaction
    )
    {
        await ExecuteAsync(interaction);
    }

    [SlashCommand(
        "violent",
        "Sends an anime GIF (violent)."
    )]
    [UsedImplicitly]
    public async Task ViolentCommand(
        [Choice("angry", "angry")]
        [Choice("baka", "baka")]
        [Choice("bite", "bite")]
        [Choice("bonk", "bonk")]
        [Choice("bully", "bully")]
        [Choice("chase", "chase")]
        [Choice("die", "die")]
        [Choice("kick", "kick")]
        [Choice("kill", "kill")]
        [Choice("midfing", "midfing")]
        [Choice("poke", "poke")]
        [Choice("punch", "punch")]
        [Choice("shoot", "shoot")]
        [Choice("slap", "slap")]
        [Choice("stab", "stab")]
        [Choice("suicide", "suicide")]
        string interaction
    )
    {
        await ExecuteAsync(interaction);
    }

    [SlashCommand(
        "emotion",
        "Sends an anime GIF (emotions and expressions)."
    )]
    [UsedImplicitly]
    public async Task EmotionCommand(
        [Choice("angry", "angry")]
        [Choice("blush", "blush")]
        [Choice("bored", "bored")]
        [Choice("cringe", "cringe")]
        [Choice("cry", "cry")]
        [Choice("disgust", "disgust")]
        [Choice("facepalm", "facepalm")]
        [Choice("happy", "happy")]
        [Choice("laugh", "laugh")]
        [Choice("love", "love")]
        [Choice("nervous", "nervous")]
        [Choice("panic", "panic")]
        [Choice("pout", "pout")]
        [Choice("sad", "sad")]
        [Choice("shrug", "shrug")]
        [Choice("sleepy", "sleepy")]
        [Choice("smile", "smile")]
        [Choice("smug", "smug")]
        [Choice("think", "think")]
        [Choice("thumbsup", "thumbsup")]
        [Choice("triggered", "triggered")]
        string interaction
    )
    {
        await ExecuteAsync(interaction);
    }

    [SlashCommand(
        "action",
        "Sends an anime GIF (actions)."
    )]
    [UsedImplicitly]
    public async Task ActionCommand(
        [Choice("bite", "bite")]
        [Choice("bonk", "bonk")]
        [Choice("bully", "bully")]
        [Choice("chase", "chase")]
        [Choice("cheer", "cheer")]
        [Choice("dab", "dab")]
        [Choice("dance", "dance")]
        [Choice("facepalm", "facepalm")]
        [Choice("highfive", "highfive")]
        [Choice("lick", "lick")]
        [Choice("lurk", "lurk")]
        [Choice("midfing", "midfing")]
        [Choice("panic", "panic")]
        [Choice("run", "run")]
        [Choice("sip", "sip")]
        [Choice("stare", "stare")]
        [Choice("tease", "tease")]
        [Choice("think", "think")]
        [Choice("tickle", "tickle")]
        [Choice("wave", "wave")]
        [Choice("wink", "wink")]
        string interaction
    )
    {
        await ExecuteAsync(interaction);
    }

    private async Task ExecuteAsync(string interaction)
    {
        var token = cm.Get().WaifuItAccessToken;

        if (token is null)
        {
            await RespondAsync("No waifu.it access token provided. Please contact an administrator.");

            return;
        }

        await DeferAsync();

        var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + interaction);
        req.Headers.Add("Authorization", token);

        var res = (await client.SendAsync(req).ToObjectAsync<JObject>())?["url"];

        if (res is null)
        {
            await FollowupAsync("Failed to get waifu image.", ephemeral: true);

            return;
        }

        await FollowupAsync(res.ToString());
    }
}
