// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Database.Context;

namespace Numerous.Bot.Discord.Interactions.Commands.Config;

public partial class ConfigCommandModule
{
    [Group("forumfeed", "Forum feed configuration commands")]
    public sealed partial class ForumFeedCommandModule(IUnitOfWork uow) : InteractionModule
    {
        [GeneratedRegex(@"\s+")]
        private static partial Regex SpaceRegex();

        [UsedImplicitly]
        [SlashCommand("subscribe", "Specify which forum to subscribe to.")]
        public async Task Subscribe
        (
            [Summary("forum_ids", "Comma-separated list of forum IDs to subscribe to")]
            string forumIdsStr
        )
        {
            await DeferAsync(true);

            forumIdsStr = SpaceRegex().Replace(forumIdsStr, "");
            var forumIdStrings = forumIdsStr.Split(',');

            var forumIds = new List<int>();

            foreach (var forumIdString in forumIdStrings)
            {
                if (int.TryParse(forumIdString, out var forumId))
                {
                    forumIds.Add(forumId);
                }
                else
                {
                    await FollowupWithEmbedAsync(
                        "Invalid forum IDs",
                        "Please specify a comma-separated list of valid forum IDs.",
                        ResponseType.Error
                    );

                    return;
                }
            }

            await uow.MessageChannels.SetSubscribedForumsAsync(Context.Guild.Id, Context.Channel.Id, forumIds);
            await uow.CommitAsync();

            await FollowupWithEmbedAsync(
                "Subscribed to forum(s)",
                "Successfully subscribed to the specified forum(s).",
                ResponseType.Success
            );
        }
    }
}
