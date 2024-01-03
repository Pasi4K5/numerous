// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Globalization;
using System.Text.RegularExpressions;

namespace Numerous.Discord.Commands;

public partial class AnilistSearchCommandModule
{
    private const string EmojiRegexString = "<:.*?:.*?>";

    [GeneratedRegex(EmojiRegexString)]
    private static partial Regex EmojiRegex();

    [GeneratedRegex("\n" + @$"\*\*\d+\*\*{EmojiRegexString}" + "\n")]
    private static partial Regex KakeraValueRegex();

    [GeneratedRegex(@"\([^)]*\)")]
    private static partial Regex CharacterPostfixRegex();

    private static string ReplaceHtml(string s)
    {
        return s
            .Replace("<br>", "")
            .Replace("<i>", "*")
            .Replace("</i>", "*")
            .Replace("<b>", "**")
            .Replace("</b>", "**")
            .Replace("<u>", "__")
            .Replace("</u>", "__")
            .Replace("<s>", "~~")
            .Replace("</s>", "~~");
    }

    private static string MakeReadable(string? s)
    {
        return s switch
        {
            "TV_SHORT" => "TV Short",
            "NOVEL" => "Light Novel",
            "OVA" => "OVA",
            "ONA" => "ONA",
            null => "Unknown",
            _ => ToTitleCase(s),
        };
    }

    private static string ToTitleCase(string s)
    {
        return new CultureInfo("en-US").TextInfo.ToTitleCase(s.ToLower().Replace('_', ' '));
    }

    private static string ExtractCharName(string charName)
    {
        return CharacterPostfixRegex().IsMatch(charName)
            ? CharacterPostfixRegex().Replace(charName, "")[..^1]
            : charName;
    }

    private static string? ExtractMediaTitle(string? embedDesc)
    {
        if (embedDesc is null)
        {
            return null;
        }

        if (embedDesc.Contains("React with any emoji to claim!"))
        {
            int lastIndex;

            if (KakeraValueRegex().IsMatch(embedDesc))
            {
                lastIndex = KakeraValueRegex().Match(embedDesc).Index;
            }
            else
            {
                lastIndex = embedDesc.IndexOf("\nReact with any emoji to claim!", StringComparison.Ordinal);

                if (lastIndex < 0)
                {
                    return null;
                }
            }

            embedDesc = embedDesc[..lastIndex];
        }
        else
        {
            if (!EmojiRegex().IsMatch(embedDesc))
            {
                return null;
            }

            var lastIndex = EmojiRegex().Match(embedDesc).Index;

            embedDesc = embedDesc[..lastIndex];
        }

        return embedDesc.Replace('\n', ' ').Trim();
    }
}
