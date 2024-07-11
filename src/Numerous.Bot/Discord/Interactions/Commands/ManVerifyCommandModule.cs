// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.ApiClients.Osu;

namespace Numerous.Bot.Discord.Interactions.Commands;

public sealed class ManVerifyCommandModule(OsuVerifier verifier, OsuApi osu) : InteractionModule
{
    [UsedImplicitly]
    [SlashCommand("manverify", "Links the specified user's osu! account to their Discord account.")]
    public async Task ManVerify(
        [Summary("discord_user", "The Discord user to verify.")]
        IUser user,
        [Summary("osu_user", "The username or user ID of the osu! user to verify.")]
        string osuUserStr
    )
    {
        await DeferAsync();

        var isVerified = await verifier.UserIsVerifiedAsync(user);

        if (isVerified)
        {
            await FollowupWithEmbedAsync(
                message: $"{user.Mention} is already verified.",
                type: ResponseType.Info
            );

            return;
        }

        var osuUser = await osu.GetUserAsync(osuUserStr);

        if (osuUser is null)
        {
            await FollowupWithEmbedAsync(
                message: $"osu! user `{osuUserStr}` could not be found.",
                type: ResponseType.Error
            );

            return;
        }

        await verifier.VerifyAsync(user, osuUser.Id);

        await FollowupWithEmbedAsync(
            message: $"{user.Mention} has been manually verified as *[{osuUser.Username}](https://osu.ppy.sh/users/{osuUser.Id})*.",
            type: ResponseType.Success
        );
    }
}
