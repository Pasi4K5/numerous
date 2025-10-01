// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Discord.Util;

namespace Numerous.Bot.Discord.Interactions.Commands;

[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.Administrator)]
public sealed partial class PostCommandModule : InteractionModule
{
    private const string CancelBtnId = "cmd:admin:post:cancel";

    private static readonly ConcurrentDictionary<Guid, CommandState> _states = new();

    [UsedImplicitly]
    [MessageCommand("Post to channel")]
    public async Task Post(IMessage message)
    {
        if (message.Content.Length == 0)
        {
            await RespondWithEmbedAsync(
                title: "No content",
                message: "The selected message is empty.",
                ResponseType.Error,
                ephemeral: true
            );

            return;
        }

        var guid = Guid.NewGuid();
        _states[guid] = new CommandState(Context.User.Id, message.Content);

        await RespondAsync(
            embed: CreateEmbed(
                message: "Would you like to post a new message or edit an existing one?"
            ).Build(),
            components: new ComponentBuilder()
                .WithButton("Replace Existing", $"{PostEditBtnId}:{guid}", ButtonStyle.Secondary)
                .WithButton("Post New", $"{PostNewBtnId}:{guid}")
                .Build()
        );
    }

    [UsedImplicitly]
    [ComponentInteraction($"{CancelBtnId}:*", true)]
    public async Task Cancel(string guid)
    {
        if (GetState(guid).ExecutorId != Context.User.Id)
        {
            await DeferAsync();

            return;
        }

        _states.TryRemove(Guid.Parse(guid), out _);
        await Context.GetComponentInteraction().Message.ModifyAsync(msg =>
        {
            msg.Embed = new Optional<Embed>(CreateEmbed(message: "Action cancelled.").Build());
            msg.Components = new ComponentBuilder().Build();
        });
    }

    private static CommandState GetState(string guid) => _states[Guid.Parse(guid)];

    private sealed class CommandState(ulong executorId, string content)
    {
        public ulong ExecutorId { get; } = executorId;
        public string Content { get; } = content;
        public ulong? ChannelId { get; set; }
        public IUserMessage? InteractionMessage { get; set; }
        public IUserMessage? MessageToEdit { get; set; }
    }
}
