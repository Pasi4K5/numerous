// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Database;
using Numerous.Bot.Discord.Events;

namespace Numerous.Bot.Discord.Commands;

public sealed class SuperDeleteCommandModule(IDbService db, DiscordEventHandler eventHandler) : CommandModule
{
    [UsedImplicitly]
    [MessageCommand("Superdelete")]
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    public async Task SuperDelete(IMessage msg)
    {
        lock (eventHandler.SuperdeletedMessagesLock)
        {
            eventHandler.SuperdeletedMessages.Add(msg.Id);
        }

        var hideTask = HideMessageAsync(msg.Id);
        var deleteTask = msg.DeleteAsync();

        await Task.WhenAll(hideTask, deleteTask);

        await FollowupAsync($"Message `{msg.Id}` fully deleted.");
    }

    [UsedImplicitly]
    [SlashCommand("superdelete", "Deletes the given message. Also prevents it from being undeleted.")]
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    public async Task SuperDelete(
        [Summary("message", "The message to delete. Can be a message ID or a link.")]
        string msgString
    )
    {
        var msgId = msgString.ParseMessageId();

        if (msgId is null)
        {
            await FollowupAsync("Invalid message ID or link.");

            return;
        }

        List<Task> tasks = [HideMessageAsync(msgId.Value)];

        var msg = await Context.Channel.GetMessageAsync(msgId.Value);

        if (msg is not null)
        {
            lock (eventHandler.SuperdeletedMessagesLock)
            {
                eventHandler.SuperdeletedMessages.Add(msgId.Value);
            }

            tasks.Add(msg.DeleteAsync());
        }

        await Task.WhenAll(tasks);

        if (msg is not null || await db.DiscordMessages.AnyAsync(m => m.Id == msgId.Value))
        {
            await FollowupAsync($"Message `{msgId}` fully deleted.");
        }
        else
        {
            await FollowupAsync("Message not found.");
        }
    }

    private Task HideMessageAsync(ulong msgId)
    {
        return Task.WhenAll(
            DeferAsync(true),
            db.DiscordMessages.SetHiddenAsync(msgId)
        );
    }
}
