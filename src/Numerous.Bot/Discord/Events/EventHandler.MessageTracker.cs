// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Numerous.Bot.Database.Entities;
using Numerous.Bot.Util;

namespace Numerous.Bot.Discord.Events;

public partial class DiscordEventHandler
{
    public List<ulong> SuperdeletedMessages { get; } = new();
    public object SuperdeletedMessagesLock { get; } = new();

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

        lock (SuperdeletedMessagesLock)
        {
            if (SuperdeletedMessages.Remove(msgId))
            {
                return;
            }
        }

        if (channel is not IGuildChannel guildChannel)
        {
            return;
        }

        var deletedMessagesChannelId = (await db.GuildOptions.FindByIdAsync(guildChannel.GuildId))?.DeletedMessagesChannel;

        if (deletedMessagesChannelId is null)
        {
            return;
        }

        var message = await db.DiscordMessages.FindByIdAsync(msgId);

        if (message is null)
        {
            return;
        }

        var deletedMessagesChannel = await guildChannel.Guild.GetTextChannelAsync(deletedMessagesChannelId.Value);

        if (deletedMessagesChannel is null)
        {
            return;
        }

        var author = await client.Rest.GetUserAsync(message.AuthorId);

        var embed = new EmbedBuilder()
            .WithAuthor("\u2800", author.GetAvatarUrl())
            .WithDescription($"Message by {author.Mention} in <#{message.ChannelId}> was deleted.")
            .WithFields([
                new()
                {
                    Name = "Message",
                    Value = message.CleanContents.Last(),
                },
            ])
            .WithFooter($"Message ID: {msgId}")
            .WithTimestamp(message.Timestamps.First())
            .Build();

        await deletedMessagesChannel.SendMessageAsync(embed: embed);
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
