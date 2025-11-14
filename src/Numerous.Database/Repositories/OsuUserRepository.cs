// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Numerous.Database.Context;
using Numerous.Database.Dtos;
using Numerous.Database.Entities;

namespace Numerous.Database.Repositories;

public interface IOsuUserRepository : IIdRepository<OsuUserDto, int>
{
    Task<OsuUserDto?> FindByDiscordUserIdAsync(ulong discordUserId, CancellationToken ct = default);
    Task<int?> FindIdByDiscordUserIdAsync(ulong discordUserId, CancellationToken ct = default);
    Task<bool> VerifyAsync(int osuUserId, ulong discordUserId, CancellationToken ct = default);
    Task<int[]> GetVerifiedIdsAsync(CancellationToken ct = default);
    Task<Dictionary<int, ulong>> GetVerifiedWithDiscordIdAsync(CancellationToken ct = default);
}

public sealed class OsuUserRepository(NumerousDbContext context, IMapper mapper)
    : IdRepository<DbOsuUser, OsuUserDto, int>(context, mapper), IOsuUserRepository
{
    public override async Task InsertAsync(OsuUserDto dto, CancellationToken ct = default)
    {
        await EnsureDiscordUserExistsAsync(dto.DiscordUserId!.Value, ct);
        await base.InsertAsync(dto, ct);
    }

    public async Task<OsuUserDto?> FindByDiscordUserIdAsync(ulong discordUserId, CancellationToken ct = default)
    {
        return Mapper.Map<OsuUserDto>(await Set.FirstOrDefaultAsync(u => u.DiscordUserId == discordUserId, ct));
    }

    public async Task<int?> FindIdByDiscordUserIdAsync(ulong discordUserId, CancellationToken ct = default)
    {
        var result = await Set.Where(u => u.DiscordUserId == discordUserId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(ct);

        return result == default ? null : result;
    }

    public async Task<bool> VerifyAsync(int osuUserId, ulong discordUserId, CancellationToken ct = default)
    {
        var alreadyVerified = false;
        var osuUser = await Set.FindAsync([osuUserId], cancellationToken: ct);

        await EnsureDiscordUserExistsAsync(discordUserId, ct);

        if (osuUser is null)
        {
            osuUser = new DbOsuUser
            {
                Id = osuUserId,
                DiscordUserId = discordUserId,
            };

            await Set.AddAsync(osuUser, ct);
        }
        else
        {
            alreadyVerified = osuUser.DiscordUserId is not null;
            osuUser.DiscordUserId = discordUserId;
        }

        await Context.SaveChangesAsync(ct);

        return !alreadyVerified;
    }

    public async Task<int[]> GetVerifiedIdsAsync(CancellationToken ct = default)
    {
        return await Set
            .Where(u => u.DiscordUserId != null)
            .Select(u => u.Id)
            .ToArrayAsync(ct);
    }

    public async Task<Dictionary<int, ulong>> GetVerifiedWithDiscordIdAsync(CancellationToken ct = default)
    {
        return await Set
            .Where(u => u.DiscordUserId != null)
            .Select(u => new { DiscordId = u.DiscordUserId!.Value, OsuId = u.Id })
            .ToDictionaryAsync(u => u.OsuId, u => u.DiscordId, ct);
    }
}
