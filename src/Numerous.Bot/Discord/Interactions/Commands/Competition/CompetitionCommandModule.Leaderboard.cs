// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Discord.Util;
using Numerous.Database.Dtos;

namespace Numerous.Bot.Discord.Interactions.Commands.Competition;

partial class CompetitionCommandModule
{
    private const int PageSize = 5;

    private const string FirstButtonId = "cmd:competition:first";
    private const string PrevButtonId = "cmd:competition:prev";
    private const string PageButtonId = "cmd:competition:page";
    private const string PageModalId = "cmd:competition:modal";
    private const string PageModalInputId = "cmd:competition:modal:input";
    private const string NextButtonId = "cmd:competition:next";
    private const string LastButtonId = "cmd:competition:last";

    [UsedImplicitly]
    [SlashCommand("leaderboard", "Shows the leaderboard for the current beatmap competition.")]
    public async Task Leaderboard()
    {
        await DeferAsync();

        var scores = await uow.BeatmapCompetitionScores.GetCurrentLeaderboardAsync(Context.Guild.Id, PageSize, 0);
        var numScores = await uow.BeatmapCompetitionScores.GetNumTopScoresAsync(Context.Guild.Id);

        if (scores.Count == 0)
        {
            await FollowupWithEmbedAsync(
                "Empty",
                "No scores have been submitted yet. You could be the first one!"
            );

            return;
        }

        await FollowupAsync(
            embeds: BuildEmbeds(scores),
            components: BuildPaginationComponent((numScores - 1) / PageSize)
        );
    }

    [UsedImplicitly]
    [ComponentInteraction(FirstButtonId, true)]
    public async Task FirstPage()
    {
        await ModifyMessageAsync(0);
    }

    [UsedImplicitly]
    [ComponentInteraction($"{PrevButtonId}:*", true)]
    public async Task PreviousPage(int page)
    {
        await ModifyMessageAsync(Math.Max(0, page - 1));
    }

    [UsedImplicitly]
    [ComponentInteraction(PageButtonId, true)]
    public async Task CurrentPage()
    {
        var msgId = ((IComponentInteraction)Context.Interaction).Message.Id;
        await RespondWithModalAsync<PageSelectionModal>($"{PageModalId}:{msgId}");
    }

    [UsedImplicitly]
    [ModalInteraction($"{PageModalId}:*", true)]
    public async Task PageModal(string msgId, PageSelectionModal modal)
    {
        if (!int.TryParse(modal.Page, out var page))
        {
            await RespondAsync();

            return;
        }

        var message = await Context.Channel.GetMessageAsync(ulong.Parse(msgId)) as IUserMessage;

        await ModifyMessageAsync(Math.Clamp(page - 1, 0, int.MaxValue), message);
    }

    [UsedImplicitly]
    [ComponentInteraction($"{NextButtonId}:*", true)]
    public async Task NextPage(int page)
    {
        await ModifyMessageAsync(Math.Min(page + 1, int.MaxValue));
    }

    [UsedImplicitly]
    [ComponentInteraction(LastButtonId, true)]
    public async Task LastPage()
    {
        await ModifyMessageAsync(-1);
    }

    /// <param name="page"><c>-1</c> for last page.</param>
    private async Task ModifyMessageAsync(int page, IUserMessage? message = null)
    {
        message ??= Context.GetComponentInteraction().Message;
        var numScores = await uow.BeatmapCompetitionScores.GetNumTopScoresAsync(Context.Guild.Id);
        var numPages = (numScores - 1) / PageSize;

        if (page < 0 || page > numPages)
        {
            page = numPages;
        }

        var scores = await uow.BeatmapCompetitionScores
            .GetCurrentLeaderboardAsync(Context.Guild.Id, PageSize, page * PageSize);

        await message.ModifyAsync(msg =>
        {
            msg.Components = BuildPaginationComponent(numPages, page);
            msg.Embeds = BuildEmbeds(scores, page);
        });

        await RespondAsync();
    }

    private static MessageComponent BuildPaginationComponent(int maxPage, int page = 0)
    {
        var builder = new ComponentBuilder();

        if (maxPage > 0)
        {
            builder
                .WithButton(
                    customId: FirstButtonId,
                    style: ButtonStyle.Primary,
                    emote: new Emoji("⏮️"),
                    disabled: page == 0
                )
                .WithButton(
                    customId: $"{PrevButtonId}:{page}",
                    style: ButtonStyle.Primary,
                    emote: new Emoji("◀️"),
                    disabled: page == 0
                )
                .WithButton(
                    customId: PageButtonId,
                    style: ButtonStyle.Secondary,
                    label: $"{page + 1}/{maxPage + 1}"
                )
                .WithButton(
                    customId: $"{NextButtonId}:{page}",
                    style: ButtonStyle.Primary,
                    emote: new Emoji("▶️"),
                    disabled: page == maxPage
                )
                .WithButton(
                    customId: LastButtonId,
                    style: ButtonStyle.Primary,
                    emote: new Emoji("⏭️"),
                    disabled: page == maxPage
                );
        }

        return builder.Build();
    }

    private Embed[] BuildEmbeds(List<BeatmapCompetitionScoreDto> scores, int page = 0)
    {
        return scores
            .Select(s => eb.LeaderboardScore(s, scores.IndexOf(s) + page * PageSize + 1).Build())
            .ToArray();
    }

    public sealed class PageSelectionModal : IModal
    {
        public string Title => "Page Selection";

        [RequiredInput]
        [InputLabel("Page")]
        [ModalTextInput(PageModalInputId, TextInputStyle.Short, "Enter page number")]
        public string Page { get; [UsedImplicitly] set; } = "";
    }
}
