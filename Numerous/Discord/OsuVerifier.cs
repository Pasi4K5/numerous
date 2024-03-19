// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Coravel;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Numerous.ApiClients.Osu;
using Numerous.ApiClients.Osu.Models;
using Numerous.Database;
using Numerous.Database.Entities;
using Numerous.DependencyInjection;

namespace Numerous.Discord;

[SingletonService]
public sealed class OsuVerifier(IHost host, DiscordSocketClient discord, DbManager db, OsuApi osu)
{
    public Task StartAsync()
    {
        host.Services.UseScheduler(scheduler => scheduler.ScheduleAsync(async () =>
        {
            foreach (var guild in discord.Guilds)
            {
                var dbUsers = await db.Users.FindAsync(x => x.OsuId != null);

                await dbUsers.ForEachAsync(async dbUser =>
                {
                    var guildUser = guild.GetUser(dbUser.Id);

                    if (guildUser is null)
                    {
                        return;
                    }

                    await AssignRolesAsync(guildUser);
                });
            }
        }).EveryFiveSeconds());

        return Task.CompletedTask;
    }

    public async Task<bool> UserIsVerifiedAsync(IGuildUser guildUser)
    {
        var user = await db.GetUserAsync(guildUser.Id);

        return user.OsuId is not null;
    }

    public async Task<bool> OsuUserIsVerifiedAsync(OsuUser osuUser)
    {
        return await db.Users.Find(x => x.OsuId == osuUser.Id).AnyAsync();
    }

    public async Task VerifyUserAsync(IGuildUser guildUser, OsuUser osuUser)
    {
        await db.EnsureUserExistsAsync(guildUser.Id);

        await db.Users.UpdateOneAsync(
            Builders<DbUser>.Filter.Eq(x => x.Id, guildUser.Id),
            Builders<DbUser>.Update.Set(x => x.OsuId, osuUser.Id)
        );

        await AssignRolesAsync(guildUser);
    }

    public async Task SetRoleAsync(IGuild guild, OsuUserGroup group, IRole role)
    {
        var guildConfig = await (await db.GuildOptions.FindAsync(x => x.Id == guild.Id)).SingleAsync();

        guildConfig.OsuRoles.Remove(guildConfig.OsuRoles.FirstOrDefault(x => x.Group == group));
        guildConfig.OsuRoles.Add(new GuildOptions.OsuRole
        {
            Group = group,
            RoleId = role.Id,
        });

        await db.GuildOptions.UpdateOneAsync(
            Builders<GuildOptions>.Filter.Eq(x => x.Id, guild.Id),
            Builders<GuildOptions>.Update.Set(x => x.OsuRoles, guildConfig.OsuRoles)
        );
    }

    public async Task RemoveRoleAsync(IGuild guild, OsuUserGroup group)
    {
        var guildConfig = await (await db.GuildOptions.FindAsync(x => x.Id == guild.Id)).SingleAsync();

        guildConfig.OsuRoles.Remove(guildConfig.OsuRoles.FirstOrDefault(x => x.Group == group));

        await db.GuildOptions.UpdateOneAsync(
            Builders<GuildOptions>.Filter.Eq(x => x.Id, guild.Id),
            Builders<GuildOptions>.Update.Set(x => x.OsuRoles, guildConfig.OsuRoles)
        );
    }

    private async Task AssignRolesAsync(IGuildUser guildUser)
    {
        var osuUser = await GetOsuUserAsync(guildUser);

        if (osuUser is null)
        {
            return;
        }

        var guildConfig = await (await db.GuildOptions.FindAsync(x => x.Id == guildUser.GuildId)).SingleAsync();

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
            var roleId = guildConfig?.OsuRoles.FirstOrDefault(osuRole => osuRole.Group == group).RoleId;

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

    private async Task<OsuUserExtended?> GetOsuUserAsync(IUser discordUser)
    {
        var user = await db.GetUserAsync(discordUser.Id);

        if (user.OsuId is null)
        {
            return null;
        }

        return await osu.GetUserAsync(user.OsuId.Value.ToString());
    }
}
