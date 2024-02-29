// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using MongoDB.Driver;
using Numerous.Database.Entities;
using Numerous.Util;

namespace Numerous.Discord.Events;

public partial class DiscordEventHandler
{
    [Init]
    private void MessageTracker_Init()
    {
        client.MessageReceived += MessageTracker_StoreAsync;
        client.MessageDeleted += (message, _) => MessageTracker_DeleteAsync(message.Id);
        client.MessageUpdated += (_, after, _) => MessageTracker_UpdateAsync(after);
    }

    private Task MessageTracker_StoreAsync(IMessage msg)
    {
        Task.Run(async () =>
        {
            if (msg.Channel is not IGuildChannel channel
                || !(await db.GuildOptions.Find(x => x.Id == channel.GuildId).FirstOrDefaultAsync())?.TrackMessages == true)
            {
                return;
            }

            var insertTask = db.DiscordMessages.InsertOneAsync(new DiscordMessage(msg)
            {
                Id = msg.Id,
                GuildId = channel.GuildId,
            });

            var imgDirPath = cm.Get().AttachmentDirectory;

            if (!Directory.Exists(imgDirPath))
            {
                Directory.CreateDirectory(imgDirPath);
            }

            var httpClient = new HttpClient();
            var attachments = msg.Attachments.ToList();

            var storeAttachmentsTask = Task.WhenAll(attachments.Select(async attachment =>
            {
                var response = await httpClient.GetAsync(attachment.Url);
                var filePath = attachmentManager.GetTargetPath(msg.Id, attachments, attachment);

                await using var fs = new FileStream(filePath, FileMode.CreateNew);
                await response.Content.CopyToAsync(fs);
            }));

            await Task.WhenAll(insertTask, storeAttachmentsTask);
        });

        return Task.CompletedTask;
    }

    private async Task MessageTracker_DeleteAsync(ulong msgId)
    {
        if (!await ShouldUpdateMessagesAsync(msgId))
        {
            return;
        }

        await db.DiscordMessages.UpdateOneAsync(
            m => m.Id == msgId,
            Builders<DiscordMessage>.Update.Set(m => m.DeletedAt, DateTime.UtcNow)
        );
    }

    private async Task MessageTracker_UpdateAsync(IMessage newMsg)
    {
        if (!await ShouldUpdateMessagesAsync(newMsg.Id) || newMsg.Channel is not IGuildChannel || newMsg.Content is null)
        {
            return;
        }

        await db.DiscordMessages.UpdateOneAsync(
            x => x.Id == newMsg.Id,
            Builders<DiscordMessage>.Update
                .AddToSet(m => m.Timestamps, DateTime.UtcNow)
                .AddToSet(m => m.Contents, newMsg.Content)
                .AddToSet(m => m.CleanContents, newMsg.CleanContent)
        );
    }

    private async Task<bool> ShouldUpdateMessagesAsync(ulong msgId)
    {
        if (!await (await db.DiscordMessages.FindAsync(m => m.Id == msgId)).AnyAsync())
        {
            return false;
        }

        var guildId = (await (await db.DiscordMessages.FindAsync(m => m.Id == msgId)).FirstOrDefaultAsync())?.GuildId;

        if (guildId is null)
        {
            return false;
        }

        return (await db.GuildOptions.Find(x => x.Id == guildId).FirstOrDefaultAsync())?.TrackMessages == true;
    }
}
