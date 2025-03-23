// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Net;
using Discord;
using Humanizer;
using Numerous.Bot.Osu;
using Numerous.Bot.Util;
using Numerous.Bot.Web.Osu;
using Numerous.Bot.Web.Osu.Models;
using Numerous.Common.Config;
using Numerous.Database.Dtos;
using Numerous.Database.Util;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Scoring;
using Refit;

namespace Numerous.Bot.Discord.Util;

public sealed class EmbedBuilders(IConfigProvider cfgProvider, IOsuApiRepository osuApi)
{
    private static readonly OsuColour _colours = new();

    private Config Config => cfgProvider.Get();

    public static (EmbedBuilder, ComponentBuilder) BeatmapSetUpdate(
        ApiBeatmapsetExtended beatmapSet,
        string mapper,
        string[] gdMappers
    )
    {
        var (eb, cp) = BeatmapSet(beatmapSet, mapper, gdMappers);

        return (
            eb.WithTitle(beatmapSet.Ranked switch
                {
                    BeatmapOnlineStatus.WIP or BeatmapOnlineStatus.Pending =>
                        "New beatmap has been uploaded.",
                    BeatmapOnlineStatus.Qualified =>
                        "Beatmap has been qualified.",
                    BeatmapOnlineStatus.Ranked =>
                        "Beatmap has been ranked.",
                    BeatmapOnlineStatus.Loved =>
                        "Beatmap has been loved.",
                    _ => throw new InvalidOperationException("Invalid beatmap status"),
                }
            ).WithTimestamp(beatmapSet.RankedDate ?? beatmapSet.SubmittedDate),
            cp
        );
    }

    private static (EmbedBuilder, ComponentBuilder) BeatmapSet(
        ApiBeatmapsetExtended beatmapSet,
        string mapper,
        string[] gdMappers
    )
    {
        var color = OsuColour.ForBeatmapSetOnlineStatus(beatmapSet.Ranked);

        return (
            new EmbedBuilder()
                .WithColor(color?.ToRgb() ?? Color.Default)
                .WithImageUrl(beatmapSet.Covers.Card2X)
                .WithThumbnailUrl(beatmapSet.User.AvatarUrl)
                .WithDescription(
                    $"## {beatmapSet.Title}\n"
                    + $"### by {beatmapSet.Artist}\n"
                    + $"**mapped by** {mapper}\n"
                    + $"**{(gdMappers.Length == 1 ? "GD" : "GDs")} by** {gdMappers.Humanize()}"
                        .OnlyIf(gdMappers.Length > 0)
                ),
            new ComponentBuilder()
                .WithButton(
                    "Beatmap page",
                    style: ButtonStyle.Link,
                    url: $"https://osu.ppy.sh/s/{beatmapSet.Id}",
                    emote: new Emoji("🌐")
                )
                .WithButton(
                    "osu!direct",
                    style: ButtonStyle.Link,
                    url: $"https://axer-url.vercel.app/api/direct?set={beatmapSet.Id}",
                    emote: new Emoji("⬇️")
                )
                .WithButton(
                    "Mapper profile",
                    style: ButtonStyle.Link,
                    url: $"https://osu.ppy.sh/u/{beatmapSet.UserId}",
                    emote: new Emoji("👤")
                )
        );
    }

    public async Task<EmbedBuilder> CompetitionInfoAsync(WorkingBeatmap beatmap, BeatmapCompetitionDto competition)
    {
        ApiBeatmapsetExtended? apiSet;
        ApiBeatmap? apiMap;

        try
        {
            var setId = competition.LocalBeatmap.OnlineBeatmap?.OnlineBeatmapset.Id;

            apiSet = setId is not null
                ? await osuApi.GetBeatmapsetAsync(setId.Value)
                : null;
            apiMap = apiSet?.Beatmaps.FirstOrDefault(x => x.Checksum == beatmap.BeatmapInfo.MD5Hash);
        }
        catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            apiSet = null;
            apiMap = null;
        }

        var eb = Beatmap(beatmap, competition.LocalBeatmap.OnlineBeatmap, apiSet /*, apiMap*/);

        eb.Description +=
            $"\n\nThe competition ends **{competition.EndTime.ToDiscordTimestampRel()}**.";

        if (apiMap?.Checksum == beatmap.BeatmapInfo.MD5Hash)
        {
            eb.Description =
                $"-# [Download](https://osu.direct/api/d/{apiSet?.Id})"
                + $" \uff5c [osu!direct](https://axer-url.vercel.app/api/direct?map={beatmap.BeatmapInfo.OnlineID})\n"
                + eb.Description;
        }

        return eb;
    }

    private static EmbedBuilder Beatmap(
        WorkingBeatmap beatmap,
        OnlineBeatmapDto? onlineBeatmap,
        ApiBeatmapsetExtended? apiSet
        // ApiBeatmap? apiMap
    )
    {
        var meta = beatmap.Metadata;
        var sr = beatmap.GetStarRating();
        var color = _colours.ForStarDifficulty(sr).ToRgb();

        var eb = new EmbedBuilder()
            .WithColor(color)
            .WithTitle($"{meta.Artist} - {meta.Title}")
            .WithUrl($"https://osu.ppy.sh/b/{beatmap.BeatmapInfo.OnlineID}")
            .WithDescription(
                $"**Difficulty:** {beatmap.BeatmapInfo.DifficultyName}\n"
                + $"**Star Rating:** {sr:N} :star:\n"
            );

        if (apiSet is not null)
        {
            var setMapper = onlineBeatmap?.OnlineBeatmapset.Creator.DiscordUserId.HasValue == true
                ? MentionUtils.MentionUser(onlineBeatmap.OnlineBeatmapset.Creator.DiscordUserId.Value)
                : beatmap.Metadata.Author.Username.WithLink($"https://osu.ppy.sh/u/{apiSet.UserId}");
            // TODO: Implement this in a different way since beatmap owners are not being stored in the database anymore.
            // var diffMappers = onlineBeatmap?.Creators.Select(creator =>
            //     creator.DiscordUserId.HasValue
            //         ? MentionUtils.MentionUser(creator.DiscordUserId.Value)
            //         : apiSet.RelatedUsers?.FirstOrDefault(x => x.Id == apiMap?.UserId)?.Username
            //             .WithLink($"https://osu.ppy.sh/u/{apiMap!.UserId}")
            // ).Humanize();

            eb.WithImageUrl(apiSet.Covers.Card2X)
                .WithAuthor(apiSet.Creator, url: $"https://osu.ppy.sh/u/{apiSet.UserId}");
            eb.Description =
                $"**Host:** {setMapper}\n" // .OnlyIf(setMapper != diffMappers)
                // + $"**Mapper:** {diffMappers}\n"
                + eb.Description;
        }

        return eb;
    }

    public async Task<EmbedBuilder> ExtendedScoreAsync(WorkingBeatmap beatmap, BeatmapCompetitionScoreDto score, int rank)
    {
        var metadata = beatmap.Metadata;

        var eb = BasicScore(score, true);

        ApiOsuUser? user;

        try
        {
            user = await osuApi.GetUserByIdAsync(score.PlayerId);
        }
        catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            user = null;
        }

        eb.WithThumbnailUrl(user?.AvatarUrl)
            .WithTitle(metadata.Title);

        var sr = new OsuDifficultyCalculator(RulesetInfos.Osu, beatmap)
            .Calculate(score.Mods.ToModArray())
            .StarRating;

        eb.Description =
            $"\n### {metadata.Artist}\n"
            + $"**Difficulty:** {beatmap.BeatmapInfo.DifficultyName}\n"
            + $"**Mapper:** {metadata.Author.Username}\n"
            + $"**Star Rating:** {sr:N} :star:\n"
            + $"# \\#{rank} \uff5c <:_:{GetRankEmojiId(score.Rank)}> {score.TotalScore:N0}"
            + eb.Description;

        if (beatmap.BeatmapSetInfo?.OnlineID > 1)
        {
            eb.WithUrl($"https://osu.ppy.sh/b/{beatmap.BeatmapInfo.OnlineID}");

            try
            {
                var apiSet = await osuApi.GetBeatmapsetAsync((uint)beatmap.BeatmapSetInfo.OnlineID);

                eb.WithImageUrl(apiSet.Covers.Card2X);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
            }
        }

        return eb;
    }

    public EmbedBuilder LeaderboardScore(BeatmapCompetitionScoreDto score, int rank)
    {
        var eb = BasicScore(score);

        eb.WithDescription($"## \\#{rank} \uff5c <:_:{GetRankEmojiId(score.Rank)}> {score.TotalScore:N0}{eb.Description}");

        return eb;
    }

    private static EmbedBuilder BasicScore(BeatmapCompetitionScoreDto score, bool judgementsInSeparateLine = false)
    {
        var eb = new EmbedBuilder()
            .WithColor(OsuColour.ForRank(score.Rank).ToRgb())
            .WithDescription(
                $"\n{MentionUtils.MentionUser(score.Player.DiscordUserId!.Value)}"
                    .OnlyIf(score.Player.DiscordUserId.HasValue)
                + $" \uff5c [{score.DateTime.ToDiscordTimestampRel()}](https://osu.ppy.sh/scores/{score.OnlineId})"
                + (judgementsInSeparateLine ? "\n" : " \uff5c ")
                + $":blue_circle: {score.GreatCount}"
                + $" \uff5c :green_circle: {score.OkCount}"
                + $" \uff5c :yellow_circle: {score.MehCount}"
                + $" \uff5c :x: {score.MissCount}\n"
            )
            .WithFields([
                new()
                {
                    Name = ":dart: Accuracy",
                    Value = score.DisplayAccuracy
                        .BoldIf(score.Rank is ScoreRank.X or ScoreRank.XH),
                    IsInline = true,
                },
                new()
                {
                    Name = ":link: Max Combo",
                    Value = $"{score.MaxCombo}/{score.Competition.LocalBeatmap.MaxCombo}"
                        .BoldIf(score.MaxCombo == score.Competition.LocalBeatmap.MaxCombo),
                    IsInline = true,
                },
                new()
                {
                    Name = ":heavy_plus_sign: Mods",
                    Value = score.Mods.Length != 0 ? string.Join(", ", score.Mods) : "NM",
                    IsInline = true,
                },
            ]);

        if (score.OnlineId is not null)
        {
            eb.WithUrl($"https://osu.ppy.sh/scores/{score.OnlineId}");
        }

        return eb;
    }

    private ulong GetRankEmojiId(ScoreRank rank)
    {
        var emojis = Config.Emojis;

        return rank switch
        {
            ScoreRank.XH => emojis.RankSilverSs,
            ScoreRank.X => emojis.RankSs,
            ScoreRank.SH => emojis.RankSilverS,
            ScoreRank.S => emojis.RankS,
            ScoreRank.A => emojis.RankA,
            ScoreRank.B => emojis.RankB,
            ScoreRank.C => emojis.RankC,
            ScoreRank.D => emojis.RankD,
            _ => emojis.RankF,
        };
    }
}
