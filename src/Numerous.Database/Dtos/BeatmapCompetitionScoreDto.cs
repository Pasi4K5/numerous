﻿// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using JetBrains.Annotations;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace Numerous.Database.Dtos;

public sealed class BeatmapCompetitionScoreDto : IdDto<Guid>
{
    [UsedImplicitly]
    public BeatmapCompetitionScoreDto()
    {
    }

    public BeatmapCompetitionScoreDto(ScoreInfo scoreInfo, ulong? onlineId = null)
    {
        OnlineId = onlineId;
        Md5Hash = scoreInfo.Hash;
        PlayerId = scoreInfo.UserID;
        TotalScore = scoreInfo.TotalScore;
        Mods = scoreInfo.Mods.Select(m => m.Acronym).ToArray();
        Accuracy = (float)scoreInfo.Accuracy;
        MaxCombo = scoreInfo.MaxCombo;
        GreatCount = scoreInfo.Statistics.GetValueOrDefault(HitResult.Great);
        OkCount = scoreInfo.Statistics.GetValueOrDefault(HitResult.Ok);
        MehCount = scoreInfo.Statistics.GetValueOrDefault(HitResult.Meh);
        MissCount = scoreInfo.Statistics.GetValueOrDefault(HitResult.Miss);
        DateTime = scoreInfo.Date;
    }

    public string Md5Hash { get => Id.ToString("N"); init => Id = Guid.Parse(value); }

    public ulong? OnlineId { get; init; }

    public BeatmapCompetitionDto Competition { get; init; } = null!;

    public OsuUserDto Player { get; init; } = null!;
    public int PlayerId { get; init; }

    public long TotalScore { get; init; }
    public string[] Mods { get; init; } = [];
    public double Accuracy { get; init; }
    public int MaxCombo { get; init; }
    public int GreatCount { get; init; }
    public int OkCount { get; init; }
    public int MehCount { get; init; }
    public int MissCount { get; init; }
    public DateTimeOffset DateTime { get; init; }

    public string DisplayAccuracy => $"{Accuracy * 100:0.00}%";

    public ScoreRank Rank => _scoreProcessor.RankFromScore(
        Accuracy,
        new Dictionary<HitResult, int>
        {
            { HitResult.Great, GreatCount },
            { HitResult.Ok, OkCount },
            { HitResult.Meh, MehCount },
            { HitResult.Miss, MissCount },
        }
    );

    public required ReplayDto? Replay { get; init; }

    private static readonly ScoreProcessor _scoreProcessor = new OsuScoreProcessor();
}
