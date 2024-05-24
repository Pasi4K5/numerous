// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;

namespace Numerous.Bot.Discord.Interactions.Commands;

public sealed class OsuProfileCommand(OsuVerifier verifier) : InteractionModule
{
    [UsedImplicitly]
    [SlashCommand("profile", "View the osu! profile of a user.")]
    public async Task Profile_Slash(
        [Summary("user", "The user to view the profile of.")]
        IUser? user = null
    )
    {
        await Execute(user ?? Context.User, false);
    }

    [UsedImplicitly]
    [UserCommand("osu! profile")]
    public async Task Profile_User(IUser user)
    {
        await Execute(user, true);
    }

    [UsedImplicitly]
    [MessageCommand("osu! profile")]
    public async Task Profile_Message(IMessage msg)
    {
        await Execute(msg.Author, true);
    }

    private async Task Execute(IUser user, bool ephemeral)
    {
        await DeferAsync(ephemeral);

        var osuId = await verifier.GetOsuIdAsync(user);

        if (osuId is null)
        {
            await FollowupWithEmbedAsync(
                (user == Context.User ? "You are" : $"{user.Mention} is") + " not verified.",
                (user == Context.User ? "You" : "They") + " can use `/verify` to verify their osu! account."
            );

            return;
        }

        await FollowupAsync($"https://osu.ppy.sh/users/{osuId}");
    }
}
