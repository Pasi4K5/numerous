// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Database.Context;

namespace Numerous.Bot.Discord.Interactions.Commands;

public sealed class SuperDeleteCommandModule(IUnitOfWork uow) : InteractionModule
{
    [UsedImplicitly]
    [MessageCommand("Superdelete")]
    [DefaultMemberPermissions(GuildPermission.ManageMessages)]
    public async Task SuperDelete(IMessage msg)
    {
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
            tasks.Add(msg.DeleteAsync());
        }

        await Task.WhenAll(tasks);

        if (msg is not null || await uow.DiscordMessages.ExistsAsync(msgId.Value))
        {
            await FollowupAsync($"Message `{msgId}` fully deleted.");
        }
        else
        {
            await FollowupAsync("Message not found.");
        }
    }

    private async Task HideMessageAsync(ulong msgId)
    {
        await Task.WhenAll(
            DeferAsync(true),
            uow.DiscordMessages.HideAsync(msgId)
        );

        await uow.CommitAsync();
    }
}
