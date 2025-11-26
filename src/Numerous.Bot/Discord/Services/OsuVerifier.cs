// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Microsoft.Extensions.Hosting;
using Numerous.Bot.Web.Osu;
using Numerous.Bot.Web.Osu.Models;
using Numerous.Common.Enums;
using Numerous.Database.Context;
using Numerous.Database.Dtos;
using Numerous.DiscordAdapter;
using Numerous.DiscordAdapter.Guilds;
using Numerous.DiscordAdapter.Users;
using osu.Game.Beatmaps;
using Serilog;

namespace Numerous.Bot.Discord.Services;

public interface IOsuVerifier
{
    void Start(CancellationToken ct);
    Task AssignAllRolesAsync(ulong guildId, CancellationToken ct = default);
    Task VerifyAsync(ulong discordUserId, int osuUserId, CancellationToken ct = default);
    ValueTask<bool> UserIsVerifiedAsync(ulong userId, OsuUserDto[]? osuUsers = null, CancellationToken ct = default);
    Task LinkRoleAsync(ulong guildId, OsuUserGroup group, ulong roleId, CancellationToken ct = default);
    Task UnlinkRoleAsync(ulong guildId, OsuUserGroup group, CancellationToken ct = default);
}

public sealed class OsuVerifier
(
    IHost host,
    ILogger logger,
    IDiscordClient discord,
    IUnitOfWorkFactory uowFactory,
    IOsuApiRepository osuApi,
    OsuUserStatsService osuUserStats
)
    : IOsuVerifier
{
    public void Start(CancellationToken ct)
    {
        host.Services.UseScheduler(
            scheduler => scheduler.ScheduleAsync(() => AssignAllRolesAsync(ct))
                .EveryMinute()
                .PreventOverlapping(nameof(OsuVerifier))
        );
        discord.GuildMemberUpdated += async user => await AssignRolesInGuildAsync(
            user,
            await GetOsuUsersAsync(user, ct),
            await GetGroupRoleMappingsAsync(user.Guild.Id, ct),
            ct: ct
        );
        discord.GuildMemberAdd += async user => await AssignRolesInGuildAsync(
            user,
            await GetOsuUsersAsync(user, ct),
            await GetGroupRoleMappingsAsync(user.Guild.Id, ct),
            ct: ct
        );
    }

    public async Task AssignAllRolesAsync(ulong guildId, CancellationToken ct = default)
    {
        var osuUsers = await GetOsuUsersAsync(ct: ct);
        var mappings = await GetGroupRoleMappingsAsync(guildId, ct);

        foreach (var guildUser in await discord.GetGuildMembersAsync(guildId).ToArrayAsync(ct))
        {
            try
            {
                await AssignRolesInGuildAsync(guildUser, osuUsers, mappings, ct: ct);
            }
            catch (Exception e)
            {
                logger.Warning(e,
                    "Failed to assign roles to user {User} in guild {Guild}",
                    guildUser.Id, guildId
                );
            }

            if (await UserIsVerifiedAsync(guildUser.Id, osuUsers, ct))
            {
                await Task.Delay(5000, ct);
            }
        }
    }

    public async Task VerifyAsync(ulong discordUserId, int osuUserId, CancellationToken ct = default)
    {
        await using var uow = uowFactory.Create();

        if (await uow.OsuUsers.VerifyAsync(osuUserId, discordUserId, ct))
        {
            osuUserStats.StartTracking(osuUserId, ct);
        }

        await uow.CommitAsync(ct);

        await AssignRolesAsync(discordUserId, ct);
    }

    public async ValueTask<bool> UserIsVerifiedAsync
    (
        ulong userId,
        OsuUserDto[]? osuUsers = null,
        CancellationToken ct = default
    )
    {
        if (osuUsers is not null)
        {
            return osuUsers.Any(x => x.DiscordUserId == userId);
        }

        await using var uow = uowFactory.Create();

        var dbUser = await uow.OsuUsers.FindByDiscordUserIdAsync(userId, ct);

        return dbUser is not null;
    }

    public async Task LinkRoleAsync(ulong guildId, OsuUserGroup group, ulong roleId, CancellationToken ct = default)
    {
        var mapping = new GroupRoleMappingDto
        {
            GuildId = guildId,
            Group = group,
            RoleId = roleId,
        };

        await using var uow = uowFactory.Create();

        await uow.GroupRoleMappings.UpsertAsync(mapping, ct);

        await uow.CommitAsync(ct);
    }

    public async Task UnlinkRoleAsync(ulong guildId, OsuUserGroup group, CancellationToken ct = default)
    {
        await using var uow = uowFactory.Create();

        await uow.GroupRoleMappings.DeleteAsync(guildId, group, ct);

        await uow.CommitAsync(ct);
    }

    private async Task<OsuUserDto[]> GetOsuUsersAsync(IDiscordGuildMember? user = null, CancellationToken ct = default)
    {
        await using var uow = uowFactory.Create();

        return user is null
            ? await uow.OsuUsers.GetAllAsync(ct)
            : await uow.OsuUsers.FindByDiscordUserIdAsync(user.Id, ct) is { } u
                ? [u]
                : [];
    }

    private async Task<GroupRoleMappingDto[]> GetGroupRoleMappingsAsync(ulong? guildId = null, CancellationToken ct = default)
    {
        await using var uow = uowFactory.Create();

        return guildId is null
            ? await uow.GroupRoleMappings.GetAllAsync(ct)
            : await uow.GroupRoleMappings.GetByGuildAsync(guildId.Value, ct);
    }

    private async Task AssignAllRolesAsync(CancellationToken ct = default)
    {
        foreach (var guild in discord.Guilds)
        {
            await AssignAllRolesAsync(guild.Id, ct);
        }
    }

    private async Task AssignRolesAsync(ulong userId, CancellationToken ct = default)
    {
        var osuUsers = await GetOsuUsersAsync(ct: ct);
        var mappings = await GetGroupRoleMappingsAsync(ct: ct);

        foreach (var guild in discord.Guilds)
        {
            var guildUser = await discord.GetGuildMemberAsync(guild.Id, userId);

            if (guildUser is not null)
            {
                var osuUser = await GetOsuUserAsync(userId, osuUsers, ct);
                await AssignRolesInGuildAsync(guildUser, osuUsers, mappings, osuUser, ct);
            }
        }
    }

    private async Task AssignRolesInGuildAsync
    (
        IDiscordGuildMember guildMember,
        OsuUserDto[] osuUsers,
        GroupRoleMappingDto[] mappings,
        ApiOsuUser? osuUser = null,
        CancellationToken ct = default
    )
    {
        osuUser ??= await GetOsuUserAsync(guildMember.Id, osuUsers, ct);

        if (osuUser is not null)
        {
            // User has linked account -> assign verified role
            await using var uow = uowFactory.Create();
            var verifiedRoleId = await uow.Guilds.GetAsync(guildMember.Guild.Id, ct);
            var verifiedRole = guildMember.Guild.GetRole(verifiedRoleId.VerifiedRoleId ?? 0);

            if (verifiedRole is not null)
            {
                await guildMember.AddRolesAsync(verifiedRole.Id);
            }
        }

        await AssignRoleAsync(mappings, OsuUserGroup.LinkedAccount, osuUser is not null);

        if (osuUser is null)
        {
            return;
        }

        foreach (var osuRole in mappings.Where(osuRole => osuRole.Group > 0))
        {
            var role = guildMember.Guild.GetRole(osuRole.RoleId);
            var userGroups = osuUser.GetGroups();

            if (userGroups.Contains(osuRole.Group))
            {
                if (role is not null && !guildMember.RoleIds.Contains(osuRole.RoleId))
                {
                    await guildMember.AddRolesAsync(role);
                }
            }
            else if (
                role is not null
                && guildMember.RoleIds.Contains(osuRole.RoleId)
                && userGroups.All(g => mappings.FirstOrDefault(r => r.Group == g)?.RoleId != role.Id))
            {
                await guildMember.RemoveRolesAsync(role);
            }
        }

        var isRanked =
            osuUser.HasRankedSets()
            || (osuUser.HasLeaderboardGds()
                && (await osuApi.GetUserBeatmapsetsAsync(osuUser.Id, ApiBeatmapType.Guest, ct))
                .Any(
                    s => s.Ranked is BeatmapOnlineStatus.Ranked
                        or BeatmapOnlineStatus.Approved
                        or BeatmapOnlineStatus.Qualified
                )
            );

        await Task.WhenAll(
            AssignRoleAsync(mappings, OsuUserGroup.UnrankedMapper, osuUser.IsMapper() && !isRanked),
            AssignRoleAsync(mappings, OsuUserGroup.RankedMapper, isRanked),
            AssignRoleAsync(
                mappings,
                OsuUserGroup.LovedMapper,
                osuUser.HasLovedSets()
                || (osuUser.HasLeaderboardGds()
                    && (await osuApi.GetUserBeatmapsetsAsync(osuUser.Id, ApiBeatmapType.Guest, ct))
                    .Any(s => s.Ranked == BeatmapOnlineStatus.Loved)
                )
            )
        );

        return;

        async Task AssignRoleAsync(GroupRoleMappingDto[] roleMappings, OsuUserGroup group, bool add)
        {
            var roleId = roleMappings.FirstOrDefault(osuRole => osuRole.Group == group)?.RoleId;

            if (roleId is null)
            {
                return;
            }

            var role = guildMember.Guild.GetRole(roleId.Value);

            if (role is not null)
            {
                await (add ? AddRoleAsync(role) : RemoveRoleAsync(role));
            }
        }

        async Task AddRoleAsync(IDiscordRole role)
        {
            if (!guildMember.RoleIds.Contains(role.Id))
            {
                await guildMember.AddRolesAsync(role);
            }
        }

        async Task RemoveRoleAsync(IDiscordRole role)
        {
            if (guildMember.RoleIds.Contains(role.Id))
            {
                await guildMember.RemoveRolesAsync(role);
            }
        }
    }

    private async Task<ApiOsuUserExtended?> GetOsuUserAsync
    (
        ulong discordUserId,
        OsuUserDto[] osuUsers,
        CancellationToken ct = default
    )
    {
        var user = osuUsers.FirstOrDefault(x => x.DiscordUserId == discordUserId);

        if (user is null)
        {
            return null;
        }

        var osuUser = await osuApi.GetUserByIdAsync(user.Id, ct);

        if (osuUser is null)
        {
            throw new Exception(
                $"Failed to get osu! user \"{user.Id}\" "
                + $"for Discord user (ID: {discordUserId})."
            );
        }

        return osuUser;
    }
}
