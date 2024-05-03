// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Numerous.Bot.ApiClients.Osu;
using Numerous.Bot.ApiClients.Osu.Models;
using Numerous.Bot.Database;
using Numerous.Bot.Database.Entities;
using Numerous.Bot.DependencyInjection;
using Serilog;

namespace Numerous.Bot.Discord;

[SingletonService]
public sealed class OsuVerifier(IHost host, DiscordSocketClient discord, IDbService db, OsuApi osu)
{
    public Task StartAsync()
    {
        host.Services.UseScheduler(scheduler => scheduler.ScheduleAsync(AssignAllRolesAsync)
            .EveryMinute()
            .PreventOverlapping("RoleAssignment"));
        discord.GuildMemberUpdated += async (_, user) => await AssignRolesAsync(user);

        return Task.CompletedTask;
    }

    public async Task AssignAllRolesAsync()
    {
        foreach (var guild in discord.Guilds)
        {
            foreach (var guildUser in await guild.GetUsersAsync().Flatten().ToListAsync())
            {
                try
                {
                    await AssignRolesAsync(guildUser);
                }
                catch (Exception e)
                {
                    Log.Warning(e,
                        "Failed to assign roles to user {User} in guild {Guild}",
                        guildUser.Id, guild.Id
                    );
                }

                if (await UserIsVerifiedAsync(guildUser))
                {
                    await Task.Delay(2000);
                }
            }
        }
    }

    public async Task<bool> UserIsVerifiedAsync(IUser user)
    {
        var dbUser = await db.Users.FindOrInsertByIdAsync(user.Id);

        return dbUser.OsuId is not null;
    }

    public async Task<bool> OsuUserIsVerifiedAsync(OsuUser osuUser)
    {
        return await (await db.Users.FindManyAsync(x => x.OsuId == osuUser.Id)).AnyAsync();
    }

    public async Task<ulong?> GetOsuIdAsync(IUser discordUser)
    {
        return (await db.Users.FindOrInsertByIdAsync(discordUser.Id)).OsuId;
    }

    public async Task VerifyUserAsync(IGuildUser guildUser, OsuUser osuUser)
    {
        await db.Users.SetVerifiedAsync(guildUser.Id, osuUser.Id);
    }

    public async Task LinkRoleAsync(IGuild guild, OsuUserGroup group, IRole role)
    {
        var guildConfig = await db.GuildOptions.FindOrInsertByIdAsync(guild.Id);

        guildConfig.OsuRoles.Remove(guildConfig.OsuRoles.FirstOrDefault(x => x.Group == group));
        guildConfig.OsuRoles.Add(new GuildOptions.OsuRole
        {
            Group = group,
            RoleId = role.Id,
        });

        await db.GuildOptions.UpdateRolesAsync(guild.Id, guildConfig.OsuRoles);
    }

    public async Task UnlinkRoleAsync(IGuild guild, OsuUserGroup group)
    {
        var guildConfig = await db.GuildOptions.FindOrInsertByIdAsync(guild.Id);

        guildConfig.OsuRoles.Remove(guildConfig.OsuRoles.FirstOrDefault(x => x.Group == group));

        await db.GuildOptions.UpdateRolesAsync(guild.Id, guildConfig.OsuRoles);
    }

    public async Task AssignRolesAsync(IGuildUser guildUser)
    {
        var osuUser = await GetOsuUserAsync(guildUser);
        var guildConfig = await db.GuildOptions.FindOrInsertByIdAsync(guildUser.GuildId);

        await AssignRoleAsync(OsuUserGroup.Verified, osuUser is not null);

        if (osuUser is null)
        {
            return;
        }

        foreach (var osuRole in guildConfig.OsuRoles.Where(osuRole => osuRole.Group > 0))
        {
            var role = guildUser.Guild.GetRole(osuRole.RoleId);

            if (osuUser.GetGroups().Contains(osuRole.Group))
            {
                if (role is not null && !guildUser.RoleIds.Contains(osuRole.RoleId))
                {
                    await guildUser.AddRoleAsync(role);
                }
            }
            else if (role is not null && guildUser.RoleIds.Contains(osuRole.RoleId))
            {
                await guildUser.RemoveRoleAsync(role);
            }
        }

        await Task.WhenAll(
            AssignRoleAsync(OsuUserGroup.UnrankedMapper, osuUser.IsUnrankedMapper()),
            AssignRoleAsync(OsuUserGroup.RankedMapper, osuUser.IsRankedMapper()),
            AssignRoleAsync(OsuUserGroup.ProjectLoved, osuUser.IsLovedMapper())
        );

        return;

        async Task AssignRoleAsync(OsuUserGroup group, bool add)
        {
            var roleId = guildConfig.OsuRoles.FirstOrDefault(osuRole => osuRole.Group == group).RoleId;

            if (roleId == default)
            {
                return;
            }

            var role = guildUser.Guild.GetRole(roleId);

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

    public async Task<IMessageChannel?> GetVerificationLogChannelAsync(IGuild guild)
    {
        var guildConfig = await db.GuildOptions.FindOrInsertByIdAsync(guild.Id);
        var channelId = guildConfig.VerificationLogChannel;

        if (channelId is null)
        {
            return null;
        }

        return await guild.GetTextChannelAsync(channelId.Value);
    }

    public async Task SetVerificationLogChannelAsync(SocketGuild guild, IMessageChannel? channel)
    {
        await db.GuildOptions.SetVerificationLogChannel(guild.Id, channel?.Id);
    }

    private async Task<OsuUserExtended?> GetOsuUserAsync(IUser discordUser)
    {
        var user = await db.Users.FindOrInsertByIdAsync(discordUser.Id);

        if (user.OsuId is null)
        {
            return null;
        }

        var osuUser = await osu.GetUserAsync(user.OsuId.Value.ToString());

        if (osuUser is null)
        {
            throw new Exception(
                $"Failed to get osu! user \"{user.OsuId}\" "
                + $"for Discord user \"{discordUser.Username}\" (ID: {discordUser.Id})."
            );
        }

        return osuUser;
    }
}
