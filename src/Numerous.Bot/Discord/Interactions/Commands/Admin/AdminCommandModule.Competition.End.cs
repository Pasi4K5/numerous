// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Discord.Util;
using Numerous.Common.Util;

namespace Numerous.Bot.Discord.Interactions.Commands.Admin;

partial class AdminCommandModule
{
    private partial class CompetitionGroup : InteractionModule
    {
        private const string CancelButtonId = "cmd:competition:end:cancel";
        private const string ConfirmButtonId = "cmd:competition:end:confirm";

        [UsedImplicitly]
        [SlashCommand("end", "Ends the current beatmap competition now.")]
        public async Task End()
        {
            await DeferAsync();

            if (!await uow.BeatmapCompetitions.HasActiveCompetitionAsync(Context.Guild.Id))
            {
                await FollowupWithEmbedAsync(
                    title: "No active competition",
                    message: "There is currently no competition running on this server."
                );

                return;
            }

            await FollowupAsync(
                embed: BuildEmbed(),
                components: BuildComponents()
            );
        }

        [UsedImplicitly]
        [ComponentInteraction($"{CancelButtonId}:*", true)]
        public async Task CancelEnd(string userId)
        {
            if (Context.User.Id.ToString() == userId)
            {
                await UpdateResponseAsync(true, true);
            }

            await RespondAsync();
        }

        [UsedImplicitly]
        [ComponentInteraction($"{ConfirmButtonId}:*", true)]
        public async Task ConfirmEnd(string userId)
        {
            if (Context.User.Id.ToString() == userId)
            {
                await uow.BeatmapCompetitions.EndCompetitionAsync(Context.Guild.Id);
                await uow.CommitAsync();

                await (
                    RespondWithEmbedAsync(
                        title: "Competition ended",
                        message: $"The current beatmap competition has been ended {DateTimeOffset.UtcNow.ToDiscordTimestampRel()}."
                    ),
                    UpdateResponseAsync(true, false)
                );
            }
            else
            {
                await RespondAsync();
            }
        }

        private async Task UpdateResponseAsync(bool disabled, bool showCancelled)
        {
            var response = ((IComponentInteraction)Context.Interaction).Message;

            await response.ModifyAsync(msg =>
            {
                msg.Components = BuildComponents(disabled);
                msg.Embed = BuildEmbed(showCancelled);
            });
        }

        private static Embed BuildEmbed(bool cancelled = false)
        {
            return new EmbedBuilder()
                .WithColor(cancelled ? Color.Blue : Color.Gold)
                .WithTitle(":warning: End competition :warning:")
                .WithDescription(
                    "Are you sure you want to manually end the current beatmap competition now?\n"
                    + "Players will no longer be able to submit scores.\n\n"
                    + "**This action cannot be undone!**"
                )
                .WithFooter(cancelled ? "Action cancelled." : "")
                .Build();
        }

        private MessageComponent BuildComponents(bool disabled = false)
        {
            return new ComponentBuilder()
                .WithButton("No, abort!", $"{CancelButtonId}:{Context.User.Id}", ButtonStyle.Secondary, disabled: disabled)
                .WithButton("Yes, I am sure.", $"{ConfirmButtonId}:{Context.User.Id}", ButtonStyle.Danger, disabled: disabled)
                .Build();
        }
    }
}
