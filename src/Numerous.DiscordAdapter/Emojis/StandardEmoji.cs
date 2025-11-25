// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace Numerous.DiscordAdapter.Emojis;

public sealed record StandardEmoji : DiscordEmoji
{
    public static readonly StandardEmoji ArrowDown = new("\u2B07\uFE0F");
    public static readonly StandardEmoji BustInSilhouette = new("\uD83D\uDC64");
    public static readonly StandardEmoji GlobeWithMeridians = new("\uD83C\uDF10");

    public string Unicode { get; }

    private StandardEmoji(string unicode)
    {
        Unicode = unicode;
    }
}
