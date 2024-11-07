// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Numerous.Bot.Util;
using Numerous.Database.Dtos;

namespace Numerous.Bot.Discord.Events;

public partial class DiscordEventHandler
{
    [Init]
    private void MessageTracker_Init()
    {
        client.MessageReceived += MessageTracker_StoreAsync;
        client.MessageDeleted += async (message, channel) =>
            await MessageTracker_DeleteAsync(message.Id, await channel.GetOrDownloadAsync());
        client.MessageUpdated += (_, after, channel) => MessageTracker_UpdateAsync(after, channel);
    }

    private async Task MessageTracker_StoreAsync(IMessage msg)
    {
        if (msg.Channel is not IGuildChannel channel)
        {
            return;
        }

        await using var uow = uowFactory.Create();

        if (!(await uow.Guilds.GetAsync(channel.GuildId)).TrackMessages)
        {
            return;
        }

        var referenceMsg = msg.Reference?.MessageId;
        var referenceMsgId = referenceMsg?.IsSpecified == true ? referenceMsg.Value.Value : (ulong?)null;

        if (referenceMsgId is not null && await uow.DiscordMessages.FindAsync(referenceMsgId.Value) is null)
        {
            var refMsg = await msg.Channel.GetMessageAsync(referenceMsgId.Value);

            if (refMsg is null)
            {
                referenceMsgId = null;
            }
            else
            {
                await MessageTracker_StoreAsync(refMsg);
            }
        }

        await uow.DiscordMessages.InsertAsync(new DiscordMessageDto
        {
            Id = msg.Id,
            AuthorId = msg.Author.Id,
            GuildId = channel.GuildId,
            ChannelId = msg.Channel.Id,
            ReferenceMessageId = referenceMsgId,
            Versions =
            [
                new()
                {
                    MessageId = msg.Id,
                    RawContent = msg.Content,
                    CleanContent = msg.CleanContent,
                    Timestamp = msg.Timestamp,
                },
            ],
        });

        await uow.CommitAsync();

        var imgDirPath = cfgProvider.Get().AttachmentDirectory;

        if (!Directory.Exists(imgDirPath))
        {
            Directory.CreateDirectory(imgDirPath);
        }

        var attachments = msg.Attachments.ToList();

        await Task.WhenAll(attachments.Select(async attachment =>
        {
            var filePath = attachmentService.GetTargetPath(msg.Id, attachment, attachments.IndexOf(attachment));
            await attachmentService.SaveAttachmentAsync(attachment.Url, filePath);
        }));
    }

    private async Task MessageTracker_DeleteAsync(ulong msgId, IMessageChannel channel)
    {
        if (!await ShouldUpdateMessagesAsync(channel))
        {
            return;
        }

        await using var uow = uowFactory.Create();

        await uow.DiscordMessages.SetDeletedAsync(msgId);

        await uow.CommitAsync();
    }

    private async Task MessageTracker_UpdateAsync(IMessage newMsg, IMessageChannel channel)
    {
        var now = DateTimeOffset.UtcNow;

        if (!await ShouldUpdateMessagesAsync(channel) || newMsg.Channel is not IGuildChannel || newMsg.Content is null)
        {
            return;
        }

        await using var uow = uowFactory.Create();

        await uow.DiscordMessageVersions.InsertAsync(new()
        {
            MessageId = newMsg.Id,
            RawContent = newMsg.Content,
            CleanContent = newMsg.CleanContent,
            Timestamp = now,
        });

        await uow.CommitAsync();
    }

    private async Task<bool> ShouldUpdateMessagesAsync(IMessageChannel channel)
    {
        if (channel is not IGuildChannel guildChannel)
        {
            return false;
        }

        await using var uow = uowFactory.Create();

        return (await uow.Guilds.FindAsync(guildChannel.GuildId))?.TrackMessages == true;
    }
}
