// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Numerous.Bot.Osu;
using Numerous.Database.Dtos;
using osu.Game.Beatmaps;
using osu.Game.Scoring;

namespace Numerous.Bot.Discord.Interactions.Commands.BeatmapCompetition;

public sealed partial class CompetitionCommandModule
{
    [UsedImplicitly]
    [SlashCommand("submit", "Submit a score by uploading a replay file.")]
    public async Task Submit(
        [Summary("replay", "The replay file to submit.")]
        IAttachment attachment
    )
    {
        await DeferAsync();

        using var client = httpClientFactory.CreateClient();
        var replayData = await client.GetByteArrayAsync(attachment.Url);
        var competition = await uow.BeatmapCompetitions.FindCurrentWithBeatmapAndCreatorAsync(Context.Guild.Id);

        if (competition is null)
        {
            await FollowupWithEmbedAsync(
                "No active competition",
                "There is currently no active beatmap competition on this server.",
                ResponseType.Error
            );

            return;
        }

        var osuUserId = await uow.OsuUsers.FindIdByDiscordUserIdAsync(Context.User.Id);

        if (osuUserId is null)
        {
            await FollowupWithEmbedAsync(
                "Not verified",
                "You must verify your osu! account before submitting scores.\n"
                + "You can do that with `/verify`",
                ResponseType.Error
            );

            return;
        }

        ScoreInfo score;
        WorkingBeatmap beatmap;

        try
        {
            beatmap = OsuParser.ParseBeatmap(competition.LocalBeatmap.OsuText);
            score = OsuParser.ParseReplay(beatmap, replayData);
        }
        catch (Exception)
        {
            await FollowupWithEmbedAsync(
                "Invalid replay",
                "The provided replay file is invalid or corrupted.",
                ResponseType.Error
            );

            return;
        }

        score.ToStandardisedScore(beatmap);

        var (result, scoreId) = await scoreValidator.ValidateAsync(competition, osuUserId.Value, score, beatmap);

        if (result != ScoreValidator.ValidationResult.Valid)
        {
            await RespondWithErrorAsync(result);

            return;
        }

        var dto = new BeatmapCompetitionScoreDto(score, onlineId: scoreId)
        {
            Replay = new()
            {
                Md5Hash = score.Hash,
                Data = replayData,
            },
        };

        await uow.BeatmapCompetitionScores.InsertAsync(competition, osuUserId.Value, dto);

        try
        {
            await uow.CommitAsync();
        }
        catch (DbUpdateException e) when (e.HResult == -2146233088)
        {
            await FollowupWithEmbedAsync(
                "Already submitted",
                "You have already submitted this score.",
                ResponseType.Error
            );

            return;
        }

        dto = await uow.BeatmapCompetitionScores.GetWithPlayerAndBeatmapAsync(dto.Id);
        var rank = await uow.BeatmapCompetitionScores.GetRankOfAsync(dto, Context.Guild.Id);

        await FollowupAsync(
            "Score submitted.",
            embed: eb.LeaderboardScore(dto, rank).Build()
        );
    }

    private async Task RespondWithErrorAsync(ScoreValidator.ValidationResult result)
    {
        var (title, message) = result switch
        {
            ScoreValidator.ValidationResult.BeatmapMismatch => (
                "Invalid score",
                "This score does not belong to the beatmap of the current beatmap competition.\n"
                + "To obtain the correct beatmap, use the `/competition info` command."
            ),
            ScoreValidator.ValidationResult.Failed => (
                "Failed score",
                "You cannot submit failed scores."
            ),
            ScoreValidator.ValidationResult.ScoreNotFound => (
                "Score not found",
                "The score you are trying to submit was not found in your recent scores.\n"
                + "Please note that you can only submit scores from the last 24 hours."
            ),
            ScoreValidator.ValidationResult.ScoreV2 => (
                "Invalid score",
                "You are not allowed to use ScoreV2."
            ),
            ScoreValidator.ValidationResult.TooEarly => (
                "Invalid score",
                "This score was set before the competition started.\n"
            ),
            ScoreValidator.ValidationResult.UsernameMismatch => (
                "Player mismatch",
                "The score you are trying to submit doesn't seem to be yours.\n"
                + "If you've changed your osu! username after setting this score, "
                + "you can unfortunately not submit it anymore."
            ),
            ScoreValidator.ValidationResult.UserNotFound => (
                "User not found",
                "Your osu! account could not be found.\n"
                + "This is either due to a temporary issue or because your account is restricted.\n"
                + "If your account is restricted, you cannot submit any scores."
            ),
            ScoreValidator.ValidationResult.Valid =>
                throw new UnreachableException("Valid result should not be handled here."),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
        };

        await FollowupWithEmbedAsync(title, message, ResponseType.Error);
    }
}
