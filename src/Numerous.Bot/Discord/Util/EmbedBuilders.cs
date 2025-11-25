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
using Numerous.Common.Util;
using Numerous.Database.Dtos;
using Numerous.Database.Util;
using Numerous.DiscordAdapter.Emojis;
using Numerous.DiscordAdapter.Messages.Components;
using Numerous.DiscordAdapter.Messages.Components.Buttons;
using Numerous.DiscordAdapter.Messages.Embeds;
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

    public static (DiscordEmbed, DiscordMessageComponent[]) BeatmapSetUpdate(
        ApiBeatmapsetExtended beatmapSet,
        string mapper,
        string[] gdMappers
    )
    {
        var (embed, components) = BeatmapSet(beatmapSet, mapper, gdMappers);

        return (
            embed.With(e =>
            {
                e.Title = beatmapSet.Ranked switch
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
                };
                e.Timestamp = beatmapSet.RankedDate ?? beatmapSet.SubmittedDate;
            }),
            components
        );
    }

    private static (DiscordEmbed, DiscordMessageComponent[]) BeatmapSet(
        ApiBeatmapsetExtended beatmapSet,
        string mapper,
        string[] gdMappers
    )
    {
        var color = OsuColour.ForBeatmapSetOnlineStatus(beatmapSet.Ranked);

        return (
            new DiscordEmbed
            {
                Color = System.Drawing.Color.FromArgb(color?.ToArgb() ?? 0),
                ImageUrl = beatmapSet.Covers.Card2X,
                ThumbnailUrl = beatmapSet.User.AvatarUrl,
                Description =
                    $"## {beatmapSet.Title}\n"
                    + $"### by {beatmapSet.Artist}\n"
                    + $"**mapped by** {mapper}\n"
                    + $"**{(gdMappers.Length == 1 ? "GD" : "GDs")} by** {gdMappers.Humanize()}"
                        .OnlyIf(gdMappers.Length > 0),
            },
            [
                new DiscordLinkButtonComponent
                {
                    Label = "Beatmap page",
                    Url = $"https://osu.ppy.sh/s/{beatmapSet.Id}",
                    Emoji = StandardEmoji.GlobeWithMeridians,
                },
                new DiscordLinkButtonComponent
                {
                    Label = "osu!direct",
                    Url = $"https://axer-url.vercel.app/api/direct?set={beatmapSet.Id}",
                    Emoji = StandardEmoji.ArrowDown,
                },
                new DiscordLinkButtonComponent
                {
                    Label = "Mapper profile",
                    Url = $"https://osu.ppy.sh/u/{beatmapSet.UserId}",
                    Emoji = StandardEmoji.BustInSilhouette,
                },
            ]
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

    public async Task<DiscordEmbed> ForumPostAsync(ApiForumTopicMeta meta, ApiForumPost post)
    {
        const string newTopicMarker = UnicodeCharacter.Sparkles;
        const string replyMarker = UnicodeCharacter.RightArrowCurvingLeft;

        var author = await osuApi.GetUserByIdAsync(post.UserId);
        var isFirstPost = post.Id == meta.FirstPostId;

        var color = isFirstPost ? 0x99eb47 : 0xff66aa;
        var marker = isFirstPost ? newTopicMarker : replyMarker;

        return new()
        {
            Color = System.Drawing.Color.FromArgb(color),
            Title = $"{marker} {meta.Title}",
            Author = new()
            {
                Name = author.Username,
                IconUrl = author.AvatarUrl,
                Url = Link.OsuUser(author.Id),
            },
            Description = MarkupTransformer.BbCodeToDiscordMd(post.Body.Raw)
                .LimitLength(CharacterLimit.DiscordEmbedDescription),
            Timestamp = post.CreatedAt,
        };
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
            + $"# \\#{rank} \uff5c {MentionRankEmote(score.Rank)} {score.TotalScore:N0}"
            + eb.Description;

        if (beatmap.BeatmapSetInfo?.OnlineID > 1)
        {
            eb.WithUrl($"https://osu.ppy.sh/b/{beatmap.BeatmapInfo.OnlineID}");

            try
            {
                var apiSet = await osuApi.GetBeatmapsetAsync(beatmap.BeatmapSetInfo.OnlineID);

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

        eb.WithDescription($"## \\#{rank} \uff5c {MentionRankEmote(score.Rank)} {score.TotalScore:N0}{eb.Description}");

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

    private string MentionRankEmote(ScoreRank rank)
    {
        var emojis = Config.Emojis;

        return DiscordUtil.MentionEmote(rank switch
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
        });
    }

    public static EmbedBuilder UserTimeoutKickLog(IUser usr) =>
        UserLog(usr, "User Kicked", "has been kicked for not verifying in time.");

    private static EmbedBuilder UserLog(IUser user, string title, string text) =>
        new EmbedBuilder()
            .WithColor(Color.Blue)
            .WithTitle(title)
            .WithDescription($"\n{user.Mention} (`{user.Username}`) {text}")
            .WithThumbnailUrl(user.GetAvatarUrl());
}
