// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Numerous.Bot.Web.Osu.Models;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace Numerous.Bot.Osu;

public static class ScoreExtensions
{
    public static bool MatchesApiScore(this ScoreInfo scoreInfo, ApiScore apiScore)
    {
        var scoreStats = scoreInfo.Statistics;
        var apiStats = apiScore.Statistics;

        return
            Math.Abs(apiScore.TotalScore - scoreInfo.TotalScore) <= 1
            && Math.Abs(apiScore.Accuracy - scoreInfo.Accuracy) < 1E-5
            && apiScore.MaxCombo == scoreInfo.MaxCombo
            && apiScore.Beatmap?.Checksum == scoreInfo.BeatmapHash
            && apiStats.Great == scoreStats.GetValueOrDefault(HitResult.Great)
            && apiStats.Ok == scoreStats.GetValueOrDefault(HitResult.Ok)
            && apiStats.Meh == scoreStats.GetValueOrDefault(HitResult.Meh)
            && apiStats.Miss == scoreStats.GetValueOrDefault(HitResult.Miss);
    }
}
