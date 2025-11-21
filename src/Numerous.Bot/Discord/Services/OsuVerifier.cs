// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Numerous.Bot.Web.Osu;
using Numerous.Bot.Web.Osu.Models;
using Numerous.Common.Enums;
using Numerous.Database.Context;
using Numerous.Database.Dtos;
using osu.Game.Beatmaps;
using Serilog;

namespace Numerous.Bot.Discord.Services;

public sealed class OsuVerifier
(
    IHost host,
    ILogger logger,
    DiscordSocketClient discord,
    IUnitOfWorkFactory uowFactory,
    IOsuApiRepository osuApi,
    OsuUserStatsService osuUserStats
)
{
    public void Start(CancellationToken ct)
    {
        host.Services.UseScheduler(scheduler => scheduler.ScheduleAsync(() => AssignAllRolesAsync(ct))
            .EveryMinute()
            .PreventOverlapping(nameof(OsuVerifier)));
        discord.GuildMemberUpdated += async (_, user) => await AssignRolesInGuildAsync(
            user,
            await GetOsuUsersAsync(user, ct),
            await GetGroupRoleMappingsAsync(user.Guild, ct),
            ct: ct
        );
        discord.UserJoined += async user => await AssignRolesInGuildAsync(
            user,
            await GetOsuUsersAsync(user, ct),
            await GetGroupRoleMappingsAsync(user.Guild, ct),
            ct: ct
        );
    }

    public async Task AssignAllRolesAsync(SocketGuild guild, CancellationToken ct = default)
    {
        var osuUsers = await GetOsuUsersAsync(ct: ct);
        var mappings = await GetGroupRoleMappingsAsync(guild, ct);

        foreach (var guildUser in await guild.GetUsersAsync().Flatten().ToListAsync(ct))
        {
            try
            {
                await AssignRolesInGuildAsync(guildUser, osuUsers, mappings, ct: ct);
            }
            catch (Exception e)
            {
                logger.Warning(e,
                    "Failed to assign roles to user {User} in guild {Guild}",
                    guildUser.Id, guild.Id
                );
            }

            if (await UserIsVerifiedAsync(guildUser, osuUsers, ct))
            {
                await Task.Delay(5000, ct);
            }
        }
    }

    public async Task VerifyAsync(IUser discordUser, int osuUserId, CancellationToken ct = default)
    {
        await using var uow = uowFactory.Create();

        if (await uow.OsuUsers.VerifyAsync(osuUserId, discordUser.Id, ct))
        {
            osuUserStats.StartTracking(osuUserId, ct);
        }

        await uow.CommitAsync(ct);

        await AssignRolesAsync(discordUser, ct);
    }

    public async ValueTask<bool> UserIsVerifiedAsync(IUser user, OsuUserDto[]? osuUsers = null, CancellationToken ct = default)
    {
        if (osuUsers is not null)
        {
            return osuUsers.Any(x => x.DiscordUserId == user.Id);
        }

        await using var uow = uowFactory.Create();

        var dbUser = await uow.OsuUsers.FindByDiscordUserIdAsync(user.Id, ct);

        return dbUser is not null;
    }

    public async Task LinkRoleAsync(IGuild guild, OsuUserGroup group, IRole role, CancellationToken ct = default)
    {
        var mapping = new GroupRoleMappingDto
        {
            GuildId = guild.Id,
            Group = group,
            RoleId = role.Id,
        };

        await using var uow = uowFactory.Create();

        await uow.GroupRoleMappings.UpsertAsync(mapping, ct);

        await uow.CommitAsync(ct);
    }

    public async Task UnlinkRoleAsync(IGuild guild, OsuUserGroup group, CancellationToken ct = default)
    {
        await using var uow = uowFactory.Create();

        await uow.GroupRoleMappings.DeleteAsync(guild.Id, group, ct);

        await uow.CommitAsync(ct);
    }

    private async Task<OsuUserDto[]> GetOsuUsersAsync(IGuildUser? user = null, CancellationToken ct = default)
    {
        await using var uow = uowFactory.Create();

        return user is null
            ? await uow.OsuUsers.GetAllAsync(ct)
            : await uow.OsuUsers.FindByDiscordUserIdAsync(user.Id, ct) is { } u
                ? [u]
                : [];
    }

    private async Task<GroupRoleMappingDto[]> GetGroupRoleMappingsAsync(IGuild? guild = null, CancellationToken ct = default)
    {
        await using var uow = uowFactory.Create();

        return guild is null
            ? await uow.GroupRoleMappings.GetAllAsync(ct)
            : await uow.GroupRoleMappings.GetByGuildAsync(guild.Id, ct);
    }

    private async Task AssignAllRolesAsync(CancellationToken ct = default)
    {
        foreach (var guild in discord.Guilds)
        {
            await AssignAllRolesAsync(guild, ct);
        }
    }

    private async Task AssignRolesAsync(IUser user, CancellationToken ct = default)
    {
        var osuUsers = await GetOsuUsersAsync(ct: ct);
        var mappings = await GetGroupRoleMappingsAsync(ct: ct);

        foreach (var guild in discord.Guilds)
        {
            var guildUser = (IGuildUser)guild.GetUser(user.Id);

            if (guildUser is not null)
            {
                var osuUser = await GetOsuUserAsync(user, osuUsers, ct);
                await AssignRolesInGuildAsync(guildUser, osuUsers, mappings, osuUser, ct);
            }
        }
    }

    private async Task AssignRolesInGuildAsync(IGuildUser guildUser, OsuUserDto[] osuUsers, GroupRoleMappingDto[] mappings, ApiOsuUser? osuUser = null, CancellationToken ct = default)
    {
        osuUser ??= await GetOsuUserAsync(guildUser, osuUsers, ct);

        if (osuUser is not null)
        {
            // User has linked account -> assign verified role
            await using var uow = uowFactory.Create();
            var verifiedRoleId = await uow.Guilds.GetAsync(guildUser.GuildId, ct);
            var verifiedRole = guildUser.Guild.GetRole(verifiedRoleId.VerifiedRoleId ?? 0);

            if (verifiedRole is not null)
            {
                await guildUser.AddRoleAsync(verifiedRole);
            }
        }

        await AssignRoleAsync(mappings, OsuUserGroup.LinkedAccount, osuUser is not null);

        if (osuUser is null)
        {
            return;
        }

        foreach (var osuRole in mappings.Where(osuRole => osuRole.Group > 0))
        {
            var role = guildUser.Guild.GetRole(osuRole.RoleId);
            var userGroups = osuUser.GetGroups();

            if (userGroups.Contains(osuRole.Group))
            {
                if (role is not null && !guildUser.RoleIds.Contains(osuRole.RoleId))
                {
                    await guildUser.AddRoleAsync(role);
                }
            }
            else if (
                role is not null
                && guildUser.RoleIds.Contains(osuRole.RoleId)
                && userGroups.All(g => mappings.FirstOrDefault(r => r.Group == g)?.RoleId != role.Id))
            {
                await guildUser.RemoveRoleAsync(role);
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

            var role = guildUser.Guild.GetRole(roleId.Value);

            if (role is not null)
            {
                await (add ? AddRoleAsync(role) : RemoveRoleAsync(role));
            }
        }

        async Task AddRoleAsync(IRole role)
        {
            if (!guildUser.RoleIds.Contains(role.Id))
            {
                await guildUser.AddRoleAsync(role);
            }
        }

        async Task RemoveRoleAsync(IRole role)
        {
            if (guildUser.RoleIds.Contains(role.Id))
            {
                await guildUser.RemoveRoleAsync(role);
            }
        }
    }

    private async Task<ApiOsuUserExtended?> GetOsuUserAsync(IUser discordUser, OsuUserDto[] osuUsers, CancellationToken ct = default)
    {
        var user = osuUsers.FirstOrDefault(x => x.DiscordUserId == discordUser.Id);

        if (user is null)
        {
            return null;
        }

        var osuUser = await osuApi.GetUserByIdAsync(user.Id, ct);

        if (osuUser is null)
        {
            throw new Exception(
                $"Failed to get osu! user \"{user.Id}\" "
                + $"for Discord user \"{discordUser.Username}\" (ID: {discordUser.Id})."
            );
        }

        return osuUser;
    }
}
