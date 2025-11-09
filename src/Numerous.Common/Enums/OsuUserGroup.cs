// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.Interactions;
using JetBrains.Annotations;

namespace Numerous.Common.Enums;

public enum OsuUserGroup : sbyte
{
    [UsedImplicitly] [ChoiceDisplay("Linked Account")] LinkedAccount = 0,
    [UsedImplicitly] [ChoiceDisplay("Unranked Mapper")] UnrankedMapper = -1,
    [UsedImplicitly] [ChoiceDisplay("Ranked Mapper")] RankedMapper = -2,
    [UsedImplicitly] [ChoiceDisplay("Loved Mapper")] LovedMapper = -3,
    [UsedImplicitly] [ChoiceDisplay("GMT")] GlobalModerationTeam = 4,
    [UsedImplicitly] [ChoiceDisplay("NAT")] NominationAssessmentTeam = 7,
    [UsedImplicitly] [ChoiceDisplay("Developers")] Developers = 11,
    [UsedImplicitly] [ChoiceDisplay("Community Contributors")] OsuAlumin = 16,
    [UsedImplicitly] [ChoiceDisplay("Technical Support Team")] TechnicalSupportTeam = 17,
    [UsedImplicitly] [ChoiceDisplay("Beatmap Nominators")] BeatmapNominators = 28,
    [UsedImplicitly] [ChoiceDisplay("Project Loved")] ProjectLoved = 31,
    [UsedImplicitly] [ChoiceDisplay("Beatmap Nominators (Probationary)")] BeatmapNominatorsProbationary = 32,
    [UsedImplicitly] [ChoiceDisplay("ppy")] Ppy = 33,
    [UsedImplicitly] [ChoiceDisplay("Featured Artists")] FeaturedArtists = 35,
    [UsedImplicitly] [ChoiceDisplay("Beatmap Spotlight Curators")] BeatmapSpotlightCurators = 48,
}
