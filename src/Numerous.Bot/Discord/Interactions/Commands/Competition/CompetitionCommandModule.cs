// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using Numerous.Bot.Discord.Util;
using Numerous.Bot.Osu;
using Numerous.Database.Context;

namespace Numerous.Bot.Discord.Interactions.Commands.Competition;

[Group("competition", "Beatmap competition commands.")]
[CommandContextType(InteractionContextType.Guild)]
public sealed partial class CompetitionCommandModule(
    IUnitOfWork uow,
    BeatmapService beatmapService,
    EmbedBuilders eb,
    IHttpClientFactory httpClientFactory,
    ScoreValidator scoreValidator
) : InteractionModule;
