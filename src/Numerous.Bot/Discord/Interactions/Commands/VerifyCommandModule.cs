// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Common.Enums;
using Numerous.Common.Services;
using Numerous.Database.Context;

namespace Numerous.Bot.Discord.Interactions.Commands;

public sealed class VerifyCommandModule(IConfigService cfg, IUnitOfWork uow, OsuVerifier verifier) : InteractionModule
{
    [UsedImplicitly]
    [SlashCommand("verify", "Links your osu! account to your Discord account.")]
    public async Task Verify()
    {
        await DeferAsync();

        var config = cfg.Get();
        var roles = (await uow.GroupRoleMappings.GetByGuildAsync(Context.Guild.Id));
        var unrankedMapper = roles.FirstOrDefault(x => x.Group == OsuUserGroup.UnrankedMapper)?.RoleId;
        var rankedMapper = roles.FirstOrDefault(x => x.Group == OsuUserGroup.RankedMapper)?.RoleId;
        var bn = roles.FirstOrDefault(x => x.Group == OsuUserGroup.BeatmapNominators)?.RoleId;
        var rolesExist = new[] { unrankedMapper, rankedMapper, bn }.All(x => x is not null && x.Value != default);
        var verifiedRole = roles.FirstOrDefault(x => x.Group == OsuUserGroup.Verified)?.RoleId;

        var isVerified = await verifier.UserIsVerifiedAsync(Context.User);

        await FollowupWithEmbedAsync(
            message:
            $"## Click [here]({config.BaseUrl}) to verify your osu! account!\n"
            + "If you are verified...\n"
            + (verifiedRole is not null && verifiedRole.Value != default ? $"* you will receive the <@&{verifiedRole.Value}> role as well as the associated badge (role icon).\n" : "")
            + $"* you will automatically receive osu!-related roles{(rolesExist ? $" (like <@&{rankedMapper}>/<@&{unrankedMapper}>, <@&{bn}>, etc.)" : "")}\n"
            + "* you will be able to participate in more events and activities.\n"
            + "* you will be able to use more osu!-related features here on this Discord server.\n"
            + "* other Discord members will be able to see public information about your osu! profile by using commands."
            + (isVerified ? $"\n*Note: {Context.User.Mention}, you are already verified. Doing this again will not change anything for you.*" : ""),
            type: ResponseType.Info
        );
    }
}
