// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.ApiClients.Osu;

namespace Numerous.Discord.Commands;

public sealed class VerifyCommandModule(OsuApi osu, OsuVerifier verifier) : CommandModule
{
    [UsedImplicitly]
    [SlashCommand("verify", "Verifies your osu! account.")]
    public async Task Verify(
        [Summary("user", "Your osu! username or user ID")]
        string username
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

        var osuUser = await osu.GetUserAsync(username);

        if (osuUser is null)
        {
            await FollowupWithEmbedAsync(
                "User not found.",
                type: ResponseType.Error
            );

            return;
        }

        if (!string.Equals(osuUser.DiscordUsername, Context.User.Username, StringComparison.OrdinalIgnoreCase))
        {
            await FollowupWithEmbedAsync(
                "Verification failed",
                "Please make sure that the provided osu! username or ID is correct and that "
                + $"the \"Discord\" field on your osu! profile matches your Discord username (\"{Context.User.Username}\").\n"
                + "You can change that in your [osu! account settings](https://osu.ppy.sh/home/account/edit) under \"Profile\"\u2192\"discord\".\n"
                + "After the verification, you can remove it again if you want to.\n\n"
                + "**Important:**\n"
                + "* Do not enter another person's Discord username in your osu! profile. Otherwise they will be able to verify as you.\n"
                + "* You can only perform this verification once.",
                ResponseType.Error
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
            type: ResponseType.Success
        );
    }
}
