// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Discord.Util;

namespace Numerous.Bot.Discord.Interactions.Commands;

partial class PostCommandModule
{
    private const string PostNewBtnId = "cmd:admin:post:new";
    private const string ChannelSelectId = "cmd:admin:post:channel_select";
    private const string ConfirmNewBtnId = "cmd:admin:post:new_confirm";

    [UsedImplicitly]
    [ComponentInteraction($"{PostNewBtnId}:*", true)]
    public async Task PostNew(string guid)
    {
        if (GetState(guid).ExecutorId != Context.User.Id)
        {
            await DeferAsync();

            return;
        }

        await Context.GetComponentInteraction().Message.ModifyAsync(msg =>
        {
            msg.Embeds = Array.Empty<Embed>();
            msg.Components = CreateChannelSelectComponent(guid);
        });
    }

    [UsedImplicitly]
    [ComponentInteraction($"{ChannelSelectId}:*", true)]
    public async Task ChannelSelect(string guid, string[] values)
    {
        var state = GetState(guid);

        if (state.ExecutorId != Context.User.Id)
        {
            await DeferAsync();

            return;
        }

        var selectedChannelId = ulong.Parse(values[0]);
        state.ChannelId = selectedChannelId;

        await Context.GetComponentInteraction().Message.ModifyAsync(msg =>
            msg.Components = CreateChannelSelectComponent(guid)
        );

        await DeferAsync();
    }

    [UsedImplicitly]
    [ComponentInteraction($"{ConfirmNewBtnId}:*", true)]
    public async Task ConfirmNew(string guid)
    {
        var state = GetState(guid);

        if (state.ExecutorId != Context.User.Id)
        {
            await DeferAsync();

            return;
        }

        if (state.ChannelId is null)
        {
            return;
        }

        await Context.GetComponentInteraction().Message.ModifyAsync(msg =>
        {
            msg.Embed = CreateEmbed(message: "Posting message...").Build();
        });

        var channel = (IMessageChannel)await Context.Client.GetChannelAsync(state.ChannelId.Value);
        await channel.SendFilesAsync(
            await GetAttachmentsAsync(state.Message),
            state.Message.Content
        );
        _states.TryRemove(Guid.Parse(guid), out _);

        await Context.GetComponentInteraction().Message.ModifyAsync(msg =>
        {
            msg.Embed = CreateEmbed(message: $"Message successfully posted to {channel.Mention()}.").Build();
            msg.Components = new ComponentBuilder().Build();
        });

        await DeferAsync();
    }

    private static MessageComponent CreateChannelSelectComponent(string guid)
    {
        var channelSelected = GetState(guid).ChannelId is not null;

        return new ComponentBuilder()
            .AddRow(new ActionRowBuilder()
                .WithSelectMenu(
                    $"{ChannelSelectId}:{guid}",
                    type: ComponentType.ChannelSelect,
                    channelTypes: [ChannelType.Text]
                )
            )
            .AddRow(new ActionRowBuilder()
                .WithButton(
                    "Cancel",
                    $"{CancelBtnId}:{guid}",
                    ButtonStyle.Danger,
                    disabled: !channelSelected
                )
                .WithButton(
                    "Post",
                    $"{ConfirmNewBtnId}:{guid}",
                    disabled: !channelSelected
                )
            )
            .Build();
    }
}
