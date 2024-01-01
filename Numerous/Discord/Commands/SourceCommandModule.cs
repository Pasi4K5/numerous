// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.Interactions;
using JetBrains.Annotations;

namespace Numerous.Discord.Commands;

public sealed class SourceCommandModule : CommandModule
{
    private const string SourceUrl = "https://github.com/Pasi4K5/numerous";

    [UsedImplicitly]
    [SlashCommand("source", "Links to the source code.")]
    public async Task Source()
    {
        await RespondAsync("I am open source! You can find my source code here:\n" + SourceUrl);
    }
}
