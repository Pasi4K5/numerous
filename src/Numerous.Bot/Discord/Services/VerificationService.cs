// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Coravel;
using Microsoft.Extensions.Hosting;
using Numerous.Database.Context;
using Numerous.DiscordAdapter;
using Numerous.DiscordAdapter.Channels;
using Numerous.DiscordAdapter.Users;

namespace Numerous.Bot.Discord.Services;

public sealed class VerificationService
{
    private readonly IOsuVerifier _osuVerifier;
    private readonly IUnitOfWork _uow;

    private static readonly TimeSpan _verificationTimeout = TimeSpan.FromHours(1);

    public VerificationService
    (
        IHost host,
        IDiscordClient discordClient,
        IOsuVerifier osuVerifier,
        IUnitOfWorkFactory uowFactory,
        IUnitOfWork uow)
    {
        _osuVerifier = osuVerifier;
        _uow = uow;

        host.Services.UseScheduler(s => s.Schedule(() =>
            Parallel.ForEachAsync(discordClient.Guilds, async (guild, ct) =>
            {
                await using var localUow = uowFactory.Create();
                var dbGuild = await localUow.Guilds.GetAsync(guild.Id, ct);
                var verifiedRoleId = dbGuild.VerifiedRoleId;

                if (verifiedRoleId is null)
                {
                    return;
                }

                var members = discordClient.SearchGuildMembersAsync(guild.Id, GuildMemberSortType.MemberSinceNewestFirst);

                await foreach (var member in members)
                {
                    if (
                        member.JoinedAt is null
                        || member.IsBot
                        || member.RoleIds.Contains(verifiedRoleId.Value)
                        || await _osuVerifier.UserIsVerifiedAsync(member.Id, ct: ct)
                    )
                    {
                        continue;
                    }

                    if (member.JoinedAt.Value.Add(_verificationTimeout) >= DateTimeOffset.UtcNow)
                    {
                        continue;
                    }

                    await member.KickAsync("Failed to verify within the time limit.");
                    var logChannel = dbGuild.UserLogChannelId is not null
                        ? (IDiscordTextChannel)await discordClient.GetChannelAsync(dbGuild.UserLogChannelId.Value)
                        : null;

                    if (logChannel is not null)
                    {
                        await logChannel.SendMessageAsync(new()
                        {
                            Embeds =
                            [
                                new()
                                {
                                    Title = "User Kicked for Failed Verification",
                                    Description = $"{member.Mention} was kicked for failing to verify within the time limit.",
                                    Color = Color.Orange,
                                    Timestamp = DateTimeOffset.UtcNow,
                                },
                            ],
                        });
                    }
                }
            })
        ).HourlyAt(0));
    }

    public async Task HandleUserJoined(IDiscordGuildMember user)
    {
        var guild = await _uow.Guilds.GetAsync(user.Guild.Id);

        if (guild.VerifiedRoleId is null)
        {
            return;
        }

        if (await _osuVerifier.UserIsVerifiedAsync(user.Id))
        {
            await AssignMemberRoleAsync();
        }

        return;

        async Task AssignMemberRoleAsync()
        {
            var role = user.Guild.GetRole(guild.VerifiedRoleId!.Value);

            if (role is not null)
            {
                await user.AddRolesAsync(role);
            }
        }
    }
}
