// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.ApiClients.Osu;

namespace Numerous.Discord.Commands;

public sealed partial class VerifyCommandModule(OsuApi osu, OsuVerifier verifier) : CommandModule
{
    [GeneratedRegex(@"osu\.ppy\.sh/u(?:sers)?/(\d+)")]
    private static partial Regex OsuUserUrlRegex();

    [UsedImplicitly]
    [SlashCommand("verify", "Verifies your osu! account.")]
    public async Task Verify(
        [Summary("user", "Your osu! username, user ID or profile link")]
        string user
    )
    {
        await DeferAsync();

        var guildUser = Context.Guild.GetUser(Context.User.Id);

        if (await verifier.UserIsVerifiedAsync(guildUser))
        {
            await FollowupWithEmbedAsync(
                "You are already verified.",
                type: ResponseType.Info
            );

            return;
        }

        var match = OsuUserUrlRegex().Match(user);

        if (match.Success)
        {
            user = match.Groups[1].Value;
        }

        var osuUser = await osu.GetUserAsync(user);

        if (osuUser is null)
        {
            await FollowupWithEmbedAsync(
                "Verification failed",
                "User not found.",
                type: ResponseType.Error
            );

            return;
        }

        if (!string.Equals(osuUser.DiscordUsername, Context.User.Username, StringComparison.OrdinalIgnoreCase))
        {
            await FollowupWithEmbedAsync(
                "Verification failed",
                $"**To verify, please fill in your Discord username (*{Context.User.Username}*) into the *Discord* field on your osu! profile.**\n"
                + "You can do that in your [osu! account settings](https://osu.ppy.sh/home/account/edit) under *Profile*\u2192*discord*.\n\n"
                + "After the verification, you **can** remove your Discord username from your profile **if you want to**.\n\n"
                + "If that doesn't work, please double-check if the provided osu! **username**, **ID** or **profile link** is correct.\n\n"
                + "**Important:**\n"
                + "* Do **not** put another person's Discord username into the *Discord* field of your profile. Otherwise they will be able to verify as you.\n"
                + "* You can only perform this verification **once**.\n",
                ResponseType.Warning
            );

            return;
        }

        if (await verifier.OsuUserIsVerifiedAsync(osuUser))
        {
            await FollowupWithEmbedAsync(
                message: "This osu! account is already verified. "
                         + "If you believe this is a mistake or you have lost access to your Discord account, please contact a server administrator.",
                type: ResponseType.Error
            );

            return;
        }

        await verifier.VerifyUserAsync(Context.Guild.GetUser(Context.User.Id), osuUser);

        await FollowupWithEmbedAsync(
            "Verification successful",
            $"You have successfully verified as [**{osuUser.Username}**](https://osu.ppy.sh/users/{osuUser.Id}).",
            ResponseType.Success
        );

        await verifier.AssignRolesAsync(guildUser);
    }
}
