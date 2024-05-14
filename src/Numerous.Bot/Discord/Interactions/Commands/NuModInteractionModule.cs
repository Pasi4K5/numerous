// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Database;
using Numerous.Bot.Discord.Services;

namespace Numerous.Bot.Discord.Interactions.Commands;

public sealed class NuModInteractionModule(IDbService db) : InteractionModule
{
    [UsedImplicitly]
    [ComponentInteraction($"{NuModComponentBuilder.DeleteMessageId}:*,*,*,*")]
    public async Task DeleteMessage(string reportChannelIdStr, string reportMsgIdStr, string channelIdStr, string msgIdStr)
    {
        if (
            !ulong.TryParse(reportChannelIdStr, out var reportChannelId)
            || !ulong.TryParse(reportMsgIdStr, out var reportMsgId)
            || !ulong.TryParse(channelIdStr, out var channelId)
            || !ulong.TryParse(msgIdStr, out var msgId))
        {
            await FollowupWithEmbedAsync("Invalid channel or message ID.", type: ResponseType.Error);

            return;
        }

        await DeferAsync();

        var channel = await Context.Client.GetChannelAsync(channelId) as IMessageChannel;

        if (channel is null)
        {
            await FollowupWithEmbedAsync(message: $"{Context.User.Mention} Channel not found.", type: ResponseType.Error);

            return;
        }

        var msg = await channel.GetMessageAsync(msgId);

        if (msg is null)
        {
            await FollowupWithEmbedAsync(message: $"{Context.User.Mention} Message not found.", type: ResponseType.Error);

            return;
        }

        await msg.DeleteAsync();
        await db.DiscordMessages.SetHiddenAsync(msgId);

        await ResolveAsync(reportChannelId, reportMsgId, true);
    }

    [UsedImplicitly]
    [ComponentInteraction($"{NuModComponentBuilder.ResolveId}:*,*")]
    public async Task Resolve(string reportChannelIdStr, string reportMsgIdStr)
    {
        if (
            !ulong.TryParse(reportChannelIdStr, out var reportChannelId)
            || !ulong.TryParse(reportMsgIdStr, out var reportMsgId))
        {
            await FollowupWithEmbedAsync("Invalid channel or message ID.", type: ResponseType.Error);

            return;
        }

        await DeferAsync();

        await ResolveAsync(reportChannelId, reportMsgId);
    }

    private async Task ResolveAsync(ulong reportChannelId, ulong reportMsgId, bool messageDeleted = false)
    {
        var reportChannel = await Context.Client.GetChannelAsync(reportChannelId) as IMessageChannel;
        var reportMsg = reportChannel is null ? null : await reportChannel.GetMessageAsync(reportMsgId) as IUserMessage;

        if (reportMsg is not null)
        {
            await reportMsg.ModifyAsync(orig =>
            {
                orig.Components = NuModComponentBuilder.BuildDisabledDeleteComponents();
                orig.Embeds = new[]
                {
                    NuModComponentBuilder.BuildNsfwWarningEmbed(
                        reportMsg.Embeds.FirstOrDefault()?.Description ?? "",
                        reportMsg.Embeds.FirstOrDefault()?.Fields.FirstOrDefault().Value ?? ""
                    ),
                    new EmbedBuilder()
                        .WithTitle("Resolved")
                        .WithDescription(messageDeleted
                            ? $"Message was deleted by {Context.User.Mention}."
                            : $"Marked as resolved by {Context.User.Mention}."
                        ).WithTimestamp(DateTimeOffset.UtcNow)
                        .WithColor(Color.Blue)
                        .Build(),
                };
            });
        }
    }
}
