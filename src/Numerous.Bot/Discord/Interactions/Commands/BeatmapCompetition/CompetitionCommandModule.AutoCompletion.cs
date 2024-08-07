// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Globalization;
using Discord;
using Discord.Interactions;

namespace Numerous.Bot.Discord.Interactions.Commands.BeatmapCompetition;

public sealed partial class CompetitionCommandModule
{
    private class DateTimeAutocompleteHandler : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction interaction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            var value = interaction.Data.Current.Value.ToString();

            var result = new List<DateTime>();

            if (TimeOnly.TryParse(value, out var time))
            {
                var dateTime = DateTime.UtcNow.Date.Add(time.ToTimeSpan());

                if (dateTime < DateTime.UtcNow)
                {
                    dateTime = dateTime.AddDays(1);
                }

                result.Add(dateTime);
            }
            else if (DateTime.TryParse(value, out var dateTime))
            {
                result.Add(dateTime);
            }
            else if (string.IsNullOrWhiteSpace(value))
            {
                result.Add(GetSuggestedDateTime(interaction) ?? DateTime.UtcNow);
            }

            return Task.FromResult(AutocompletionResult.FromSuccess(result.Select(x => new AutocompleteResult(
                x.ToString("dddd, dd MMMM yyyy HH:mm (UTC)"),
                x.ToString(CultureInfo.InvariantCulture)
            ))));
        }

        protected virtual DateTime? GetSuggestedDateTime(IAutocompleteInteraction interaction)
        {
            return null;
        }
    }

    private sealed class WeeklyDateTimeAutocompleteHandler : DateTimeAutocompleteHandler
    {
        protected override DateTime? GetSuggestedDateTime(IAutocompleteInteraction interaction)
        {
            var val = interaction.Data.Options.ElementAt(2).Value.ToString();

            return DateTime.TryParse(val, out var dateTime)
                ? dateTime.AddDays(7)
                : null;
        }
    }
}
