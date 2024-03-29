﻿// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;

namespace Numerous.Discord.Commands;

public sealed class OsuProfileCommand(OsuVerifier verifier) : CommandModule
{
    [UsedImplicitly]
    [SlashCommand("profile", "View the osu! profile of a user.")]
    public async Task Profile(
        [Summary("user", "The user to view the profile of.")]
        IUser user
    )
    {
        await Execute(user, false);
    }

    [UsedImplicitly]
    [UserCommand("osu! profile")]
    public async Task OsuProfile(IUser user)
    {
        await Execute(user, true);
    }

    private async Task Execute(IUser user, bool ephemeral)
    {
        await DeferAsync(ephemeral);

        var osuId = await verifier.GetOsuIdAsync(user);

        if (osuId is null)
        {
            await FollowupWithEmbedAsync(
                "This user is not verified.",
                "They can use `/verify` to verify their osu! account."
            );

            return;
        }

        await FollowupAsync($"https://osu.ppy.sh/users/{osuId}");
    }
}
