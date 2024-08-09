// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Osu;

namespace Numerous.Bot.Discord.Interactions.Commands.Competition;

partial class CompetitionCommandModule
{
    [UsedImplicitly]
    [SlashCommand("info", "Provides information about the current beatmap competition.")]
    public async Task Info()
    {
        await DeferAsync();

        var competition = await uow.BeatmapCompetitions.FindCurrentWithBeatmapAndCreatorAsync(Context.Guild.Id);

        if (competition is null)
        {
            await FollowupWithEmbedAsync(
                "No competition",
                "There is currently no active beatmap competition in this server."
            );

            return;
        }

        var beatmap = OsuParser.ParseBeatmap(competition.LocalBeatmap.OsuText);
        var oszHash = competition.LocalBeatmap.OszHash;
        var metadata = beatmap.Metadata;

        var embedBuilder = await eb.CompetitionInfoAsync(beatmap, competition);

        var finalEmbed = embedBuilder.Build();
        var tempEmbed = embedBuilder.WithFooter(
            "\u231b The .osz file is being uploaded and will be available shortly."
        ).Build();

        await FollowupAsync(embed: tempEmbed);

        await FollowupWithFileAsync(
            beatmapService.GetOszStream(oszHash),
            $"{beatmap.BeatmapSetInfo.OnlineID} {metadata.Artist} - {metadata.Title}.osz"
        );

        await ModifyOriginalResponseAsync(response =>
        {
            response.Embed = finalEmbed;
        });
    }
}
