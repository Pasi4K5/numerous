// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Osu;

namespace Numerous.Bot.Discord.Interactions.Commands.Competition;

partial class CompetitionCommandModule
{
    [UsedImplicitly]
    [SlashCommand("topscore", "Shows a player's top score for the current beatmap competition.")]
    public async Task TopScore
    (
        [Summary("user", "The user to show the top score for. If left empty, it will show your score.")]
        IUser? user = null
    )
    {
        await DeferAsync();

        var score = await uow.BeatmapCompetitions.FindUserTopScoreWithReplayCompBeatmapAsync(
            Context.Guild.Id,
            user?.Id ?? Context.User.Id
        );

        if (score is null)
        {
            await FollowupWithEmbedAsync(
                "Score not found",
                "You haven't set a score for the current competition yet.",
                ResponseType.Error
            );

            return;
        }

        var rank = await uow.BeatmapCompetitionScores.GetRankOfAsync(score, Context.Guild.Id);

        var replay = score.Replay;
        var beatmap = OsuParser.ParseBeatmap(score.Competition.LocalBeatmap.OsuText);
        var embed = (await eb.ExtendedScoreAsync(beatmap, score, rank)).Build();

        if (replay is not null)
        {
            await FollowupWithFileAsync(
                new MemoryStream(replay.Data),
                $"{replay.Md5Hash}.osr",
                embed: embed
            );
        }
        else
        {
            await FollowupAsync(embed: embed);
        }
    }
}
