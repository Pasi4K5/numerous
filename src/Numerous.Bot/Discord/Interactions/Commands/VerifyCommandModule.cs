// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.ApiClients.Osu;
using Numerous.Bot.Configuration;
using Numerous.Bot.Database;

namespace Numerous.Bot.Discord.Interactions.Commands;

public sealed class VerifyCommandModule(IConfigService cfg, IDbService db) : InteractionModule
{
    [UsedImplicitly]
    [SlashCommand("verify", "Verifies your osu! account.")]
    public async Task Verify()
    {
        await DeferAsync();

        var config = cfg.Get();
        var roles = (await db.GuildOptions.FindByIdAsync(Context.Guild.Id))?.OsuRoles;
        var unrankedMapper = roles?.FirstOrDefault(x => x.Group == OsuUserGroup.UnrankedMapper).RoleId;
        var rankedMapper = roles?.FirstOrDefault(x => x.Group == OsuUserGroup.RankedMapper).RoleId;
        var bn = roles?.FirstOrDefault(x => x.Group == OsuUserGroup.BeatmapNominators).RoleId;
        var rolesExist = new[] { unrankedMapper, rankedMapper, bn }.All(x => x is not null && x.Value != default);
        var verifiedRole = roles?.FirstOrDefault(x => x.Group == OsuUserGroup.Verified);

        await FollowupWithEmbedAsync(
            "Verify your osu! account",
            $"Click [here]({config.BaseUrl}) to verify your osu! account.\n\n"
            + "If you are verified, then...\n"
            + "* you will automatically get your osu!-related roles"
            + (rolesExist ? $" (<@&{rankedMapper}>/<@&{unrankedMapper}>, <@&{bn}>, etc.)" : "")
            + ".\n* other Discord members will be able to see your osu! profile by using commands.\n"
            + (verifiedRole is not null && verifiedRole.Value != default ? $"* you will receive the <@&{verifiedRole.Value.RoleId}> role as well as the associated badge (role icon).\n" : ""),
            type: ResponseType.Info
        );
    }
}
