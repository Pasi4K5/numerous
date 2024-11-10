// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Database.Context;

namespace Numerous.Bot.Discord.Interactions.Commands;

[UsedImplicitly]
public sealed class SetTimeZoneCommandModule(IUnitOfWork uow) : InteractionModule
{
    private const string TimeZoneParamName = "timezone";

    [UsedImplicitly]
    [SlashCommand("settimezone", "Sets your time zone.")]
    public async Task SetTimeZoneCommand(
        [Autocomplete<TimeZoneAutocompleteHandler>]
        [Summary(TimeZoneParamName, "Your time zone")]
        string tzId
    )
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(tzId);

        await DeferAsync(true);

        await uow.DiscordUsers.SetTimezoneAsync(Context.User.Id, tz);
        await uow.CommitAsync();

        await FollowupWithEmbedAsync($"Your time zone has been set to {tz.DisplayName}.");
    }

    [UsedImplicitly]
    private sealed class TimeZoneAutocompleteHandler : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services)
        {
            var query = autocompleteInteraction.Data.Options
                            .FirstOrDefault(x => x.Name == TimeZoneParamName)
                            ?.Value.ToString()
                        ?? "";
            var timezones = TimeZoneInfo.GetSystemTimeZones();
            var results = timezones
                .Where(x => (x.DisplayName + x.Id).Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(x => new AutocompleteResult(x.DisplayName, x.Id))
                .OrderBy(x => x.Name)
                .Take(25);

            return Task.FromResult(AutocompletionResult.FromSuccess(results));
        }
    }
}
