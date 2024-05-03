// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Database;

namespace Numerous.Bot.Discord.Commands;

[UsedImplicitly]
public sealed class SetTimeZoneCommandModule(IDbService db) : CommandModule
{
    private const string SelectMenuId = "cmd:settimezone:select:menu";
    private const string FirstButtonId = "cmd:settimezone:select:first";
    private const string PrevButtonId = "cmd:settimezone:select:prev";
    private const string PageButtonId = "cmd:settimezone:select:page";
    private const string NextButtonId = "cmd:settimezone:select:next";
    private const string LastButtonId = "cmd:settimezone:select:last";
    private const string CancelButtonId = "cmd:settimezone:select:cancel";
    private const string ConfirmButtonId = "cmd:settimezone:select:confirm";

    private static readonly Dictionary<ulong, State> _userStates = new();

    private static byte TopPage => (byte)(TimeZoneInfo.GetSystemTimeZones().Count / 25);

    private State CurrentState => _userStates[Context.User.Id];

    private TimeZoneInfo? SelectedTimeZone => CurrentState.SelectedTimeZoneId is { } id
        ? TimeZoneInfo.FindSystemTimeZoneById(id)
        : null;

    private MessageComponent Component
    {
        get
        {
            var options = TimeZoneInfo.GetSystemTimeZones()
                .Select(tz => new SelectMenuOptionBuilder(tz.DisplayName, tz.Id))
                .Skip(CurrentState.Page * 25)
                .Take(25)
                .ToList();

            return new ComponentBuilder
            {
                ActionRows =
                [
                    new ActionRowBuilder().WithSelectMenu(SelectMenuId, options, SelectedTimeZone?.DisplayName),
                    new ActionRowBuilder()
                        .WithButton("\u25c0\u25c0", FirstButtonId, disabled: CurrentState.Page <= 0)
                        .WithButton("\u25c0", PrevButtonId, disabled: CurrentState.Page <= 0)
                        .WithButton($"Page {CurrentState.Page + 1}/{TopPage + 1}", PageButtonId, ButtonStyle.Secondary, disabled: true)
                        .WithButton("\u25b6", NextButtonId, disabled: CurrentState.Page >= TopPage)
                        .WithButton("\u25b6\u25b6", LastButtonId, disabled: CurrentState.Page >= TopPage),
                    new ActionRowBuilder()
                        .WithButton("Cancel", CancelButtonId, ButtonStyle.Danger)
                        .WithButton("Confirm", ConfirmButtonId, ButtonStyle.Success, disabled: SelectedTimeZone is null),
                ],
            }.Build();
        }
    }

    [UsedImplicitly]
    [SlashCommand("settimezone", "Sets your time zone.")]
    public async Task SetTimeZoneCommand()
    {
        _userStates[Context.User.Id] = new State(
            async msg => await ModifyOriginalResponseAsync(msg)
        );

        await RespondAsync("", components: Component, ephemeral: true);
    }

    [UsedImplicitly]
    [ComponentInteraction(SelectMenuId)]
    public async Task SelectTimeZone(string id)
    {
        CurrentState.SelectedTimeZoneId = id;

        await UpdateSelectMenu();
    }

    [UsedImplicitly]
    [ComponentInteraction(FirstButtonId)]
    public async Task FirstPage()
    {
        CurrentState.Page = 0;

        await UpdateSelectMenu();
    }

    [UsedImplicitly]
    [ComponentInteraction(PrevButtonId)]
    public async Task PrevPage()
    {
        if (CurrentState.Page > 0)
        {
            CurrentState.Page--;

            await UpdateSelectMenu();
        }
    }

    [UsedImplicitly]
    [ComponentInteraction(PageButtonId)]
    public async Task Page()
    {
        await RespondAsync();
    }

    [UsedImplicitly]
    [ComponentInteraction(NextButtonId)]
    public async Task NextPage()
    {
        if (CurrentState.Page < 25)
        {
            CurrentState.Page++;

            await UpdateSelectMenu();
        }
    }

    [UsedImplicitly]
    [ComponentInteraction(LastButtonId)]
    public async Task LastPage()
    {
        CurrentState.Page = TopPage;

        await UpdateSelectMenu();
    }

    [UsedImplicitly]
    [ComponentInteraction(CancelButtonId)]
    public async Task Cancel()
    {
        await CurrentState.ModifyOriginalResponseAsync(msg =>
        {
            msg.Components = new ComponentBuilder().Build();
            msg.Embed = new EmbedBuilder()
                .WithTitle("Canceled")
                .WithColor(Color.DarkRed)
                .Build();
        });

        _userStates.Remove(Context.User.Id);
    }

    [UsedImplicitly]
    [ComponentInteraction(ConfirmButtonId)]
    public async Task Confirm()
    {
        await DeferAsync(true);

        await db.Users.SetTimezoneAsync(Context.User.Id, SelectedTimeZone);

        await CurrentState.ModifyOriginalResponseAsync(msg =>
        {
            msg.Components = new ComponentBuilder().Build();
            msg.Embed = new EmbedBuilder()
                .WithTitle("Time Zone Set")
                .WithDescription($"Your time zone has been set to **{SelectedTimeZone?.DisplayName}**.")
                .WithColor(Color.DarkGreen)
                .Build();
        });
    }

    private async Task UpdateSelectMenu()
    {
        await CurrentState.ModifyOriginalResponseAsync(msg =>
        {
            msg.Components = Component;
        });

        await RespondAsync();
    }

    private record State(Func<Action<MessageProperties>, Task<IUserMessage>> ModifyOriginalResponseAsync)
    {
        public byte Page { get; set; }
        public string? SelectedTimeZoneId { get; set; }
    }
}
