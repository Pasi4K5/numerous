// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Util;
using Numerous.Database.Context;
using Numerous.Database.Dtos;

namespace Numerous.Bot.Discord.Interactions.Commands;

[UsedImplicitly]
public sealed class UnDeleteCommandModule(IUnitOfWork uow, AttachmentService attachmentService) : InteractionModule
{
    private const string PreviousButtonId = "cmd:undelete:previous";
    private const string NextButtonId = "cmd:undelete:next";

    private static readonly Dictionary<ulong, List<DiscordMessageDto>> _messageCache = new();

    [UsedImplicitly]
    [SlashCommand("undelete", "Reveals the last deleted message in this channel.")]
    public async Task UnDelete(ITextChannel? channel = null)
    {
        await DeferAsync();

        var channelId = channel?.Id ?? Context.Channel.Id;

        var messages = await uow.DiscordMessages.GetOrderedDeletedMessagesWithLastVersionAsync(channelId);
        messages.Reverse();

        await RemoveForbiddenMessages(messages);

        _messageCache[Context.Channel.Id] = messages;

        var message = messages.FirstOrDefault();

        if (message is null)
        {
            await FollowupAsync("No deleted messages found in this channel.");

            return;
        }

        var followupMsg = await FollowupAsync("Loading...");

        await AddComponentsToMessage(followupMsg, message);
    }

    private async Task AddComponentsToMessage(IUserMessage target, DiscordMessageDto msg)
    {
        var user = await Context.Client.Rest.GetUserAsync(msg.AuthorId);

        var msgContent = msg.Versions.LastOrDefault()?.RawContent;

        var embed = new EmbedBuilder()
            .WithAuthor(user.Username, user.GetAvatarUrl())
            .WithDescription($"**User ID:** {msg.AuthorId}\n"
                             + $"**Message ID:** {msg.Id}")
            .WithColor(new(0xff0000))
            .WithFields(
                new EmbedFieldBuilder()
                    .WithName("Message text")
                    .WithValue(
                        string.IsNullOrEmpty(msgContent)
                            ? "*(No message content)*"
                            : msgContent + "\n-# (edited)".OnlyIf(msg.Versions.Count > 1)
                    ),
                new EmbedFieldBuilder()
                    .WithName("Sent")
                    .WithValue($"<t:{msg.Versions.First().Timestamp.ToUnixTimeSeconds()}:R>")
                    .WithIsInline(true),
                new EmbedFieldBuilder()
                    .WithName("Deleted")
                    .WithValue($"<t:{msg.DeletedAt!.Value.ToUnixTimeSeconds()}:R>")
                    .WithIsInline(true)
            )
            .Build();

        var messages = _messageCache[Context.Channel.Id];

        await RemoveForbiddenMessages(messages, messages.IndexOf(msg));

        var prevDisabled = messages.IndexOf(msg) == messages.Count - 1;
        var nextDisabled = messages.IndexOf(msg) == 0;
        var prevMsg = prevDisabled ? null : messages[messages.IndexOf(msg) + 1];
        var nextMsg = nextDisabled ? null : messages[messages.IndexOf(msg) - 1];

        var buttons = new ComponentBuilder()
            .WithRows([
                new ActionRowBuilder().WithButton(
                    "\u2b06\ufe0f Previous",
                    $"{PreviousButtonId}:{target.Id},{prevMsg?.Id}",
                    disabled: prevDisabled
                ),
                new ActionRowBuilder().WithButton(
                    "\u2b07\ufe0f Next",
                    $"{NextButtonId}:{target.Id},{nextMsg?.Id}",
                    disabled: nextDisabled
                ),
            ])
            .Build();

        await target.ModifyAsync(m =>
        {
            m.Content = "";
            m.Components = buttons;
            m.Embed = embed;
            m.Attachments = attachmentService.GetFileAttachments(msg.Id).Select(a => (FileAttachment)a).ToArray();
        });
    }

    [UsedImplicitly]
    [ComponentInteraction($"{PreviousButtonId}:*,*")]
    public async Task Previous(string responseMsgId, string nextMsgId)
    {
        if (await Context.Channel.GetMessageAsync(ulong.Parse(responseMsgId)) is not IUserMessage responseMsg)
        {
            return;
        }

        var nextMsg = _messageCache[Context.Channel.Id].First(m => m.Id == ulong.Parse(nextMsgId));

        await AddComponentsToMessage(responseMsg, nextMsg);

        await RespondAsync();
    }

    [UsedImplicitly]
    [ComponentInteraction($"{NextButtonId}:*,*")]
    public async Task Next(string responseMsgId, string nextMsgId)
    {
        if (await Context.Channel.GetMessageAsync(ulong.Parse(responseMsgId)) is not IUserMessage responseMsg)
        {
            return;
        }

        var nextMsg = _messageCache[Context.Channel.Id].First(m => m.Id == ulong.Parse(nextMsgId));

        await AddComponentsToMessage(responseMsg, nextMsg);

        await RespondAsync();
    }

    private async Task RemoveForbiddenMessages(List<DiscordMessageDto> messages, int index = 0)
    {
        var bannedUserIds = new List<ulong>();

        var banCollections = Context.Guild.GetBansAsync();

        await foreach (var bans in banCollections)
        {
            bannedUserIds.AddRange(bans.Select(b => b.User.Id));
        }

        for (var i = index; i <= index + 1; i++)
        {
            while (messages.Count > i && bannedUserIds.Contains(messages[i].AuthorId))
            {
                messages.RemoveAt(i);
            }
        }
    }
}
