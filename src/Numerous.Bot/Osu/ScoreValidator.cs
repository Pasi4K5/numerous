// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Net;
using Numerous.Bot.Web.Osu;
using Numerous.Bot.Web.Osu.Models;
using Numerous.Database.Dtos;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using Refit;

namespace Numerous.Bot.Osu;

public sealed class ScoreValidator(IOsuApiRepository osuApi)
{
    public static Type[] ForbiddenMods { get; } =
    [
        typeof(ModScoreV2),
        typeof(OsuModTargetPractice),
    ];

    public async Task<(ValidationResult result, ulong? apiScoreId)> ValidateAsync(
        BeatmapCompetitionDto competition,
        uint osuUserId,
        ScoreInfo score,
        WorkingBeatmap workingBeatmap
    )
    {
        if (score.BeatmapHash != competition.LocalBeatmap.Md5Hash)
        {
            return (ValidationResult.BeatmapMismatch, null);
        }

        if (score.Date < competition.StartTime)
        {
            return (ValidationResult.TooEarly, null);
        }

        if (score.Mods.Any(x => ForbiddenMods.Contains(x.GetType())))
        {
            return (ValidationResult.ForbiddenModCombination, null);
        }

        ApiScore[] scores;
        string apiUsername;

        try
        {
            scores = await osuApi.GetRecentScoresAsync(osuUserId);

            apiUsername = scores.Length > 0
                ? scores[0].User.Username
                : (await osuApi.GetUserByIdAsync(osuUserId)).Username;
        }
        catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return (ValidationResult.UserNotFound, null);
        }

        if (apiUsername != score.User.Username)
        {
            return (ValidationResult.UsernameMismatch, null);
        }

        if (IsFailed(score, workingBeatmap))
        {
            return (ValidationResult.Failed, null);
        }

        var scoreFound = false;

        foreach (var apiScore in scores.Where(score.MatchesApiScore))
        {
            scoreFound = true;

            if (apiScore.EndedAt > competition.StartTime)
            {
                return (ValidationResult.Valid, apiScore.Id);
            }
        }

        // No score withing the competition time frame was found.

        if (await BeatmapWasUpdatedAsync(competition))
        {
            return (ValidationResult.Valid, null);
        }

        return scoreFound
            ? (ValidationResult.TooEarly, null)
            : (ValidationResult.ScoreNotFound, null);
    }

    private async Task<bool> BeatmapWasUpdatedAsync(BeatmapCompetitionDto competition)
    {
        var apiBeatmap = await osuApi.GetBeatmapAsync(competition.LocalBeatmap.OnlineBeatmapId);

        return !competition.LocalBeatmap.Md5Hash.Equals(apiBeatmap.Checksum, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFailed(ScoreInfo score, WorkingBeatmap beatmap)
    {
        var playableBeatmap = beatmap.GetPlayableBeatmap(RulesetInfos.Osu, score.Mods);

        var expectedNumResults = playableBeatmap.HitObjects.Count(x => x.Judgement.MaxResult.AffectsCombo());
        var actualNumResults = score.MaximumStatistics[HitResult.Great];

        return expectedNumResults != actualNumResults;
    }

    public enum ValidationResult
    {
        Valid,
        ForbiddenModCombination,
        Failed,
        BeatmapMismatch,
        UsernameMismatch,
        UserNotFound,
        TooEarly,
        ScoreNotFound,
    }
}
