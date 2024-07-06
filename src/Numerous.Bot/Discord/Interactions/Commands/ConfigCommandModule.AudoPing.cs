// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Database;
using Numerous.Bot.Database.Entities;

namespace Numerous.Bot.Discord.Interactions.Commands;

public partial class ConfigCommandModule
{
    [Group("autoping", "Auto-ping configuration commands")]
    public sealed class AutoPingCommandModule(IDbService db) : InteractionModule
    {
        private const string ChannelParamName = "channel";

        [UsedImplicitly]
        [SlashCommand("add", "Adds a new auto-ping tag.")]
        public async Task Add(
            [Summary(ChannelParamName, "The forum channel to listen for new posts in.")]
            IForumChannel channel,
            [Summary("role", "The role to ping when a new post is created. Leave empty to stop pinging for this tag.")]
            IRole? role = null,
            [Autocomplete(typeof(ForumTagAutocompleteHandler))]
            [Summary("tag", "The tag to listen for in new posts. Leave empty to listen for all posts.")]
            string? tag = null
        )
        {
            await DeferAsync();

            if (role is null)
            {
                var forumTag = channel.Tags.FirstOrDefault(t => t.Id.ToString() == tag);

                if (tag is null || forumTag == default)
                {
                    await db.GuildOptions.RemoveAutoPingOptionAsync(Context.Guild.Id, channel.Id);
                }
                else
                {
                    await db.GuildOptions.RemoveAutoPingOptionAsync(Context.Guild.Id, channel.Id, forumTag.Id);
                }
            }
            else
            {
                await db.GuildOptions.AddAutoPingOptionAsync(Context.Guild.Id, new GuildOptions.AutoPingOption
                {
                    ChannelId = channel.Id,
                    RoleId = role.Id,
                    Tag = tag is not null ? ulong.Parse(tag) : null,
                });
            }

            await FollowupWithEmbedAsync($"Auto-ping configuration for {channel.Mention} has been updated.");
        }

        public sealed class ForumTagAutocompleteHandler : AutocompleteHandler
        {
            public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
                IInteractionContext context,
                IAutocompleteInteraction autocompleteInteraction,
                IParameterInfo parameter,
                IServiceProvider services)
            {
                var channelId = autocompleteInteraction.Data.Options
                    .FirstOrDefault(x => x.Name == ChannelParamName)
                    ?.Value.ToString();

                var results = new List<AutocompleteResult>();

                if (channelId is not null)
                {
                    var channel = (IForumChannel)await context.Guild.GetChannelAsync(ulong.Parse(channelId));
                    results.AddRange(channel.Tags.Select(x => new AutocompleteResult(x.Name, x.Id.ToString())));
                }

                return AutocompletionResult.FromSuccess(results.Take(25));
            }
        }
    }
}
