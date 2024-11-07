// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Security.Cryptography;
using Numerous.Bot.Web.Osu.Models;
using Numerous.Common.Config;
using Numerous.Common.Util;
using Numerous.Database.Context;

namespace Numerous.Bot.Osu;

public sealed class BeatmapService(IConfigProvider cfgProvider, IUnitOfWork uow, IHttpClientFactory httpClientFactory)
{
    private const string MirrorBaseUrl = "https://osu.direct/api";

    private Config Config => cfgProvider.Get();

    private readonly HttpClient _client = httpClientFactory.CreateClient();

    public async Task CreateCompetitionAsync(ulong guildId, ApiBeatmapExtended beatmap, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        _client.DefaultRequestHeaders.ConnectionClose = true;

        var setDownloadTask = Task.Run(async () =>
        {
            if (beatmap is not { Beatmapset: not null, Checksum: not null })
            {
                throw new InvalidOperationException("Beatmapset or checksum is null.");
            }

            var response = await _client.GetAsync($"{MirrorBaseUrl}/d/{beatmap.Beatmapset.Id}");
            var stream = await response.Content.ReadAsStreamAsync();
            var hash = await SHA256.HashDataAsync(stream);

            return (response, stream, hash);
        });

        var beatmapTask = Task.Run(async () =>
        {
            var response = await _client.GetAsync($"{MirrorBaseUrl}/osu/{beatmap.Id}");

            return await response.Content.ReadAsStringAsync();
        });

        var insertTask = Task.Run(async () =>
        {
            var (osuText, (_, _, oszHash)) = await (beatmapTask, setDownloadTask);
            var checksum = beatmap.Checksum!;

            await uow.OnlineBeatmapsets.EnsureExistsAsync(new()
            {
                Id = beatmap.Beatmapset!.Id,
                CreatorId = beatmap.Beatmapset.UserId,
            });
            await uow.OnlineBeatmaps.EnsureExistsAsync(new()
            {
                Id = beatmap.Id,
                CreatorId = beatmap.UserId,
                OnlineBeatmapsetId = beatmap.Beatmapset.Id,
            });
            await uow.LocalBeatmaps.EnsureExistsAsync(new()
            {
                Md5Hash = checksum,
                OsuText = osuText,
                OszHash = oszHash,
                MaxCombo = beatmap.MaxCombo,
                OnlineBeatmapId = beatmap.Id,
            });
            await uow.BeatmapCompetitions.InsertAsync(new()
            {
                GuildId = guildId,
                StartTime = startTime,
                EndTime = endTime,
                LocalBeatmapId = Guid.Parse(checksum),
            });

            await uow.CommitAsync();
        });

        var saveOszTask = Task.Run(async () =>
        {
            var (response, _, oszHash) = await setDownloadTask;

            if (!Directory.Exists(Config.BeatmapDirectory))
            {
                Directory.CreateDirectory(Config.BeatmapDirectory);
            }

            var fileName = $"{oszHash.ToHexString()}.osz";
            var path = Path.Combine(Config.BeatmapDirectory, fileName);

            await using var fs = File.Create(path);
            await response.Content.CopyToAsync(fs);
        });

        await TaskUtil.WhenAll(insertTask, saveOszTask);
    }

    public FileStream? GetOszStream(byte[] hash)
    {
        var fileName = $"{hash.ToHexString()}.osz";
        var path = Path.Combine(Config.BeatmapDirectory, fileName);

        return !File.Exists(path) ? null : File.OpenRead(path);
    }
}
