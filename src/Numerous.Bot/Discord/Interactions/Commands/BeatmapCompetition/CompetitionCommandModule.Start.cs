// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Net;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Util;
using Numerous.Bot.Web.Osu.Models;
using Refit;

namespace Numerous.Bot.Discord.Interactions.Commands.BeatmapCompetition;

public sealed partial class CompetitionCommandModule
{
    private const string IdGroup = "id";

    [GeneratedRegex(
        @$"^\s*((?i:(https?://)?osu\.ppy\.sh)/(b(eatmaps)?|(beatmapset)?s/[0-9]+#(osu|taiko|fruits|mania))/)?(?<{IdGroup}>[0-9]+)[/\s]*$",
        RegexOptions.ExplicitCapture | RegexOptions.NonBacktracking
    )]
    private static partial Regex BeatmapRegex();

    [UsedImplicitly]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("start", "Starts a new beatmap competition.")]
    public async Task Start(
        [Summary("beatmap", "The link or ID of the beatmap.")]
        string beatmapStr,
        [Summary("start_time", "The start time of the competition (in UTC).")]
        [Autocomplete(typeof(DateTimeAutocompleteHandler))]
        string startTimeStr,
        [Summary("end_time", "The end time of the competition (in UTC).")]
        [Autocomplete(typeof(WeeklyDateTimeAutocompleteHandler))]
        string endTimeStr
    )
    {
        if (!Context.Guild.GetUser(Context.User.Id).GuildPermissions.Administrator)
        {
            await RespondWithEmbedAsync(
                title: "Insufficient permissions.",
                message: "You must be an administrator to start a competition.",
                ResponseType.Error
            );
        }

        var match = BeatmapRegex().Match(beatmapStr);

        if (!DateTime.TryParse(startTimeStr, out var startTime))
        {
            await RespondWithEmbedAsync(
                title: "Invalid start time.",
                message: "Please provide a valid start time."
            );

            return;
        }

        if (!DateTime.TryParse(endTimeStr, out var endTime))
        {
            await RespondWithEmbedAsync(
                title: "Invalid end time.",
                message: "Please provide a valid end time."
            );

            return;
        }

        var startTimeUtc = startTime.ToOffset();
        var endTimeUtc = endTime.ToOffset();

        if (endTimeUtc <= DateTimeOffset.UtcNow)
        {
            await RespondWithEmbedAsync(
                title: "Invalid end time.",
                message: "The end time must be in the future."
            );

            return;
        }

        if (startTimeUtc >= endTimeUtc)
        {
            await RespondWithEmbedAsync(
                title: "Invalid time range.",
                message: "The start time must be before the end time."
            );

            return;
        }

        await DeferAsync();

        if (await uow.BeatmapCompetitions.GuildHasCompetitionDuringAsync(Context.Guild.Id, startTimeUtc, endTimeUtc))
        {
            await FollowupWithEmbedAsync(
                title: "Time conflict.",
                message: "There is already a competition during the specified time."
            );

            return;
        }

        if (!match.Success)
        {
            await FollowupWithEmbedAsync(
                title: "Invalid beatmap link or ID.",
                message: "Please provide a valid beatmap link or ID."
            );

            return;
        }

        var id = uint.Parse(match.Groups[IdGroup].Value);
        ApiBeatmapExtended beatmap;

        try
        {
            beatmap = await osuApi.GetBeatmapAsync(id);
        }
        catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            await FollowupWithEmbedAsync(
                title: "Beatmap not found.",
                message: "The provided beatmap link/ID could not be found."
            );

            return;
        }

        if (beatmap.Mode != ApiRuleset.Osu.Name)
        {
            await FollowupWithEmbedAsync(
                title: "Invalid game mode.",
                message: "Only osu! (standard) beatmaps are currently supported.",
                ResponseType.Error
            );

            return;
        }

        await FollowupWithEmbedAsync(
            title: ":hourglass: Downloading beatmap...",
            message: "Depending on the beatmap size, this may take a while."
        );

        await beatmapService.CreateCompetitionAsync(Context.Guild.Id, beatmap, startTimeUtc, endTimeUtc);

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Embed = new EmbedBuilder()
                .WithTitle("Competition created.")
                .WithDescription("The competition has been successfully created.")
                .WithColor(GetTypeColor(ResponseType.Success))
                .Build();
        });
    }
}
