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
        _client.MessageReceived += async msg => await MessageTracker_StoreAsync(msg);
        _client.MessageDeleted += (message, _) => MessageTracker_DeleteAsync(message.Id);
        _client.MessageUpdated += (_, after, _) => MessageTracker_UpdateAsync(after);
    }

    private async Task MessageTracker_StoreAsync(IMessage msg)
    {
        if (msg.Channel is not IGuildChannel channel || !(await _db.GuildOptions.Find(x => x.Id == channel.GuildId).FirstOrDefaultAsync())?.TrackMessages == true)
        {
            return;
        }

        await _db.DiscordMessages.InsertOneAsync(new DiscordMessage(msg)
        {
            Id = msg.Id,
            GuildId = channel.GuildId,
        });
    }

    private async Task MessageTracker_DeleteAsync(ulong msgId)
    {
        if (!await ShouldUpdateMessagesAsync(msgId))
        {
            return;
        }

        await _db.DiscordMessages.UpdateOneAsync(
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

        await _db.DiscordMessages.UpdateOneAsync(
            x => x.Id == newMsg.Id,
            Builders<DiscordMessage>.Update
                .AddToSet(m => m.Timestamps, DateTime.UtcNow)
                .AddToSet(m => m.Contents, newMsg.Content)
                .AddToSet(m => m.CleanContents, newMsg.CleanContent)
        );
    }

    private async Task<bool> ShouldUpdateMessagesAsync(ulong msgId)
    {
        if (!await (await _db.DiscordMessages.FindAsync(m => m.Id == msgId)).AnyAsync())
        {
            return false;
        }

        var guildId = (await (await _db.DiscordMessages.FindAsync(m => m.Id == msgId)).FirstOrDefaultAsync())?.GuildId;

        if (guildId is null)
        {
            return false;
        }

        return (await _db.GuildOptions.Find(x => x.Id == guildId).FirstOrDefaultAsync())?.TrackMessages == true;
    }
}
