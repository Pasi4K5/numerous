// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Discord.Util;
using Numerous.Database.Context;

namespace Numerous.Bot.Discord.Interactions.Commands.Config;

public partial class ConfigCommandModule
{
    [Group("mapfeed", "Mapfeed configuration commands")]
    public sealed class MapfeedCommandModule(IUnitOfWork uow) : InteractionModule
    {
        [UsedImplicitly]
        [SlashCommand("set-channel", "Sets the channel in which newly uploaded/qualified/ranked beatmaps should be posted.")]
        public async Task SetChannel
        (
            [Summary("channel", "The channel in which newly uploaded/qualified/ranked beatmaps should be posted.")]
            IMessageChannel? channel
        )
        {
            await DeferAsync();

            await uow.Guilds.SetMapfeedChannel(Context.Guild.Id, channel?.Id);
            await uow.CommitAsync();

            await FollowupWithEmbedAsync(
                $"Server mapfeed channel was set to {channel?.Mention() ?? "none"}.",
                type: ResponseType.Success
            );
        }
    }
}
