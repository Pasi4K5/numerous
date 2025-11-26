// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Net;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Web.Osu;
using Numerous.Bot.Web.Osu.Models;
using Numerous.Database.Context;
using Refit;

namespace Numerous.Bot.Discord.Interactions.Commands;

public sealed class MapperCommandModule(IUnitOfWork uow, IOsuApiRepository osuApi) : InteractionModule
{
    [UsedImplicitly]
    [SlashCommand("mapper", "Shows mapping-related information about a user")]
    public async Task MapperSlashCommand
    (
        [Summary("member", "The Discord user to display information about. Must be verified.")]
        IUser? discordUser = null,
        [Summary("osu_user", "The osu! username of the user to display information about. If left empty, it will display your own.")]
        string? osuUserStr = null)
    {
        await Execute(discordUser, true, osuUserStr, false);
    }

    [UsedImplicitly]
    [UserCommand("Mapper")]
    public async Task MapperUserCommand(IUser user)
    {
        await Execute(user);
    }

    [UsedImplicitly]
    [MessageCommand("Mapper")]
    public async Task MapperMessageCommand(IMessage msg)
    {
        await Execute(msg.Author);
    }

    private async Task Execute(IUser? discordUser, bool prioritizeUsername = false, string? osuUserStr = null, bool ephemeral = true)
    {
        if (discordUser is not null && osuUserStr is not null)
        {
            await RespondWithEmbedAsync(
                "Invalid arguments",
                "You can only specify either a Discord user or an osu! user, not both.",
                ResponseType.Error,
                ephemeral
            );

            return;
        }

        discordUser ??= Context.User;

        await DeferAsync(ephemeral);

        osuUserStr ??= (await uow.OsuUsers.FindByDiscordUserIdAsync(discordUser.Id))?.Id.ToString();

        if (string.IsNullOrEmpty(osuUserStr))
        {
            await FollowupWithEmbedAsync(
                "User not verified",
                "This user has not verified their osu! account yet.",
                ResponseType.Error
            );

            return;
        }

        try
        {
            var osuUser = await osuApi.GetUserAsync(osuUserStr, prioritizeUsername);

            var tasks = new[]
            {
                osuApi.GetUserBeatmapsetsAsync(osuUser.Id, ApiBeatmapType.Ranked),
                osuApi.GetUserBeatmapsetsAsync(osuUser.Id, ApiBeatmapType.Pending),
                osuApi.GetUserBeatmapsetsAsync(osuUser.Id, ApiBeatmapType.Graveyard),
            };

            var results = await Task.WhenAll(tasks);

            var rankedMaps = results[0];
            var pendingMaps = results[1];
            var allMaps = results.SelectMany(m => m).ToArray();

            var blankField = new EmbedFieldBuilder
            {
                Name = "\u200b",
                Value = "\u200b",
                IsInline = true,
            };

            List<EmbedFieldBuilder> fields =
            [
                new()
                {
                    Name = ":bust_in_silhouette: Followers",
                    Value = osuUser.FollowerCount,
                    IsInline = true,
                },
                new()
                {
                    Name = ":bell: Subscribers",
                    Value = osuUser.MappingFollowerCount,
                    IsInline = true,
                },
                blankField,
                new()
                {
                    Name = ":white_check_mark: Ranked",
                    Value = osuUser.RankedBeatmapsetCount,
                    IsInline = true,
                },
                new()
                {
                    Name = ":heart: Loved",
                    Value = osuUser.LovedBeatmapsetCount,
                    IsInline = true,
                },
                new()
                {
                    Name = ":busts_in_silhouette: GDs",
                    Value = osuUser.GuestBeatmapsetCount,
                    IsInline = true,
                },
                new()
                {
                    Name = ":hourglass: Pending",
                    Value = osuUser.PendingBeatmapsetCount,
                    IsInline = true,
                },
                new()
                {
                    Name = ":headstone: Graveyard",
                    Value = osuUser.GraveyardBeatmapsetCount,
                    IsInline = true,
                },
                blankField,
                new()
                {
                    Name = ":arrow_forward: Playcount",
                    Value = allMaps.Sum(m => m.PlayCount).ToString("N0"),
                    IsInline = true,
                },
                new()
                {
                    Name = ":white_heart: Favourites",
                    Value = allMaps.Sum(m => m.FavouriteCount).ToString("N0"),
                    IsInline = true,
                },
                new()
                {
                    Name = ":thumbs_up: Kudosu",
                    Value = osuUser.Kudosu.Total,
                    IsInline = true,
                },
            ];

            if (rankedMaps.Length > 0)
            {
                var map = rankedMaps[0];

                fields.Add(new()
                {
                    Name = "Latest Ranked Map",
                    Value = $"[{map.Artist} - {map.Title}](https://osu.ppy.sh/s/{map.Id})",
                });
            }

            if (pendingMaps.Length > 0)
            {
                var map = pendingMaps[0];

                fields.Add(new()
                {
                    Name = "Latest Pending Map",
                    Value = $"[{map.Artist} - {map.Title}](https://osu.ppy.sh/s/{map.Id})",
                });
            }

            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle("Mapper Info")
                    .WithDescription(
                        $"# [:flag_{osuUser.CountryCode.ToLower()}:](https://osu.ppy.sh/rankings/osu/performance?country={osuUser.CountryCode}) "
                        + $"[{osuUser.Username}](https://osu.ppy.sh/u/{osuUser.Id})")
                    .WithFields(fields)
                    .WithThumbnailUrl(osuUser.AvatarUrl)
                    .WithImageUrl(osuUser.Cover.Url)
                    .WithColor(0x66ccff)
                    .Build(),
                components: new ComponentBuilder()
                    .WithButton("osu! profile", url: $"https://osu.ppy.sh/u/{osuUser.Id}", style: ButtonStyle.Link)
                    .Build()
            );
        }
        catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            await FollowupWithEmbedAsync(
                "User not found",
                "The specified user could not be found.",
                ResponseType.Error
            );
        }
    }
}
