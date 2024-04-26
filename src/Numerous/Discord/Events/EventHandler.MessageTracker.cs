// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Numerous.Database.Entities;
using Numerous.Util;

namespace Numerous.Discord.Events;

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

    private Task MessageTracker_StoreAsync(IMessage msg)
    {
        Task.Run(async () =>
        {
            if (msg.Channel is not IGuildChannel channel
                || !(await db.GuildOptions.FindOrInsertByIdAsync(channel.GuildId)).TrackMessages)
            {
                return;
            }

            var insertTask = db.DiscordMessages.InsertAsync(new DiscordMessage(msg)
            {
                Id = msg.Id,
                GuildId = channel.GuildId,
            });

            var imgDirPath = cfgService.Get().AttachmentDirectory;

            if (!Directory.Exists(imgDirPath))
            {
                Directory.CreateDirectory(imgDirPath);
            }

            var httpClient = new HttpClient();
            var attachments = msg.Attachments.ToList();

            var storeAttachmentsTask = Task.WhenAll(attachments.Select(async attachment =>
            {
                var response = await httpClient.GetAsync(attachment.Url);
                var filePath = attachmentService.GetTargetPath(msg.Id, attachment, attachments.IndexOf(attachment));

                await using var fs = new FileStream(filePath, FileMode.CreateNew);
                await response.Content.CopyToAsync(fs);
            }));

            await Task.WhenAll(insertTask, storeAttachmentsTask);
        });

        return Task.CompletedTask;
    }

    private async Task MessageTracker_DeleteAsync(ulong msgId, IMessageChannel channel)
    {
        if (!await ShouldUpdateMessagesAsync(channel))
        {
            return;
        }

        await db.DiscordMessages.SetDeleted(msgId);
    }

    private async Task MessageTracker_UpdateAsync(IMessage newMsg, IMessageChannel channel)
    {
        if (!await ShouldUpdateMessagesAsync(channel) || newMsg.Channel is not IGuildChannel || newMsg.Content is null)
        {
            return;
        }

        await db.DiscordMessages.AddVersionAsync(newMsg.Id, newMsg.Content, newMsg.CleanContent);
    }

    private async Task<bool> ShouldUpdateMessagesAsync(IMessageChannel channel)
    {
        if (channel is not IGuildChannel guildChannel)
        {
            return false;
        }

        return (await db.GuildOptions.FindByIdAsync(guildChannel.GuildId))?.TrackMessages == true;
    }
}
