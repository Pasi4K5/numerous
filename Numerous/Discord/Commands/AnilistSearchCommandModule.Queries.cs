// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Numerous.Discord.Commands;

public partial class AnilistSearchCommandModule
{
    private const string CharFields =
        """
        name {
            full,
            alternative,
            alternativeSpoiler,
        },
        image: image {
            medium,
        },
        description(asHtml: false),
        gender,
        age,
        favourites,
        siteUrl,
        """;

    private const string MediaQuery =
        $$"""
        query($mediaTitle: String) {
            Media(search: $mediaTitle) {
                isAdult,
                title {
                    romaji,
                    english,
                    native,
                },
                description(asHtml: false),
                coverImage {
                    medium,
                    color,
                },
                format,
                status(version: 1),
                startDate {
                    year,
                    month,
                    day,
                },
                averageScore,
                meanScore,
                popularity,
                trending,
                favourites,
                genres,
                tags {
                    name,
                    rank,
                    isGeneralSpoiler,
                    isMediaSpoiler,
                }
                characters {
                    nodes {
                        {{CharFields}}
                    },
                },
                siteUrl,
            },
        }
        """;

    private const string CharQuery =
        $$"""
        query($charName: String) {
            Character(search: $charName) {
                {{CharFields}}
            },
        }
        """;
}
