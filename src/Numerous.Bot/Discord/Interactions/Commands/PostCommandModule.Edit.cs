// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Discord.Util;

namespace Numerous.Bot.Discord.Interactions.Commands;

[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.Administrator)]
partial class PostCommandModule
{
    private const string PostEditBtnId = "cmd:admin:post:edit";
    private const string ConfirmEditBtnId = "cmd:admin:post:edit_confirm";
    private const string MessageLinkModalId = "cmd:admin:post:modal";
    private const string MessageLinkModalInputId = "cmd:admin:post:modal_input";

    [GeneratedRegex(@"^https://discord\.com/channels/(\d+)/(\d+)/(\d+)$", RegexOptions.Compiled)]
    private static partial Regex MessageLinkRegex();

    [UsedImplicitly]
    [ComponentInteraction($"{PostEditBtnId}:*", true)]
    public async Task PostEdit(string guid)
    {
        GetState(guid).InteractionMessage = Context.GetComponentInteraction().Message;
        await RespondWithModalAsync<MessageLinkModal>($"{MessageLinkModalId}:{guid}");
    }

    [UsedImplicitly]
    [ModalInteraction($"{MessageLinkModalId}:*", true)]
    public async Task MessageLinkModalInteraction(string guid, MessageLinkModal modal)
    {
        var match = MessageLinkRegex().Match(modal.MessageLink);

        if (!match.Success)
        {
            await RespondWithEmbedAsync(
                title: "Invalid link",
                message: "The provided message link is not valid.",
                ResponseType.Error,
                ephemeral: true
            );

            return;
        }

        var guildId = ulong.Parse(match.Groups[1].Value);

        if (guildId != Context.Guild.Id)
        {
            await RespondWithEmbedAsync(
                title: "Invalid link",
                message: "The provided message link is not from this server.",
                ResponseType.Error,
                ephemeral: true
            );

            return;
        }

        var channelId = ulong.Parse(match.Groups[2].Value);
        var channel = await Context.Client.GetChannelAsync(channelId) as IMessageChannel;

        if (channel is null)
        {
            await RespondWithEmbedAsync(
                title: "Invalid link",
                message: "The channel of the provided message link could not be found.",
                ResponseType.Error,
                ephemeral: true
            );

            return;
        }

        var messageId = ulong.Parse(match.Groups[3].Value);
        var message = (IUserMessage)await channel.GetMessageAsync(messageId);

        if (message is null)
        {
            await RespondWithEmbedAsync(
                title: "Invalid link",
                message: "The provided message could not be found.",
                ResponseType.Error,
                ephemeral: true
            );

            return;
        }

        if (message.Author.Id != Context.Client.CurrentUser.Id)
        {
            await RespondWithEmbedAsync(
                title: "Invalid link",
                message: "The provided message was not sent by me.",
                ResponseType.Error,
                ephemeral: true
            );

            return;
        }

        var state = GetState(guid);
        state.MessageToEdit = message;

        await state.InteractionMessage!.ModifyAsync(msg =>
        {
            msg.Embed = CreateEmbed(
                message: "Are you sure you want to replace the message you just linked with the one you selected before?"
            ).Build();
            msg.Components = new ComponentBuilder()
                .WithButton("Cancel", $"{CancelBtnId}:{guid}", ButtonStyle.Danger)
                .WithButton("Confirm", $"{ConfirmEditBtnId}:{guid}")
                .Build();
        });

        await DeferAsync();
    }

    [UsedImplicitly]
    [ComponentInteraction($"{ConfirmEditBtnId}:*", true)]
    public async Task ConfirmEdit(string guid)
    {
        var state = GetState(guid);

        if (state.ExecutorId != Context.User.Id)
        {
            await DeferAsync();

            return;
        }

        if (state.MessageToEdit is null)
        {
            return;
        }

        await Context.GetComponentInteraction().Message.ModifyAsync(msg =>
        {
            msg.Embed = CreateEmbed(message: "Editing message...").Build();
            msg.Components = new ComponentBuilder().Build();
        });

        await state.MessageToEdit.ModifyAsync(msg => msg.Content = state.Message.Content);
        _states.TryRemove(Guid.Parse(guid), out _);

        await Context.GetComponentInteraction().Message.ModifyAsync(msg =>
        {
            msg.Embed = CreateEmbed(message: $"Message {state.MessageToEdit.GetJumpUrl()} edited successfully.").Build();
            msg.Components = new ComponentBuilder().Build();
        });

        await DeferAsync();
    }

    public sealed class MessageLinkModal : IModal
    {
        public string Title => "Message Link";

        [InputLabel("Message Link")]
        [RequiredInput]
        [ModalTextInput(MessageLinkModalInputId, placeholder: "https://discord.com/channels/...", minLength: 34, maxLength: 91)]
        public string MessageLink { get; set; } = null!;
    }
}
