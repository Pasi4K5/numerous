// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;

namespace Numerous.Bot.Util;

public static partial class MarkupTransformer
{
    [GeneratedRegex(@"\[url\](?<url>.+?)\[/url\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeSimpleUrlRegex();

    [GeneratedRegex(@"\[url=(?<url>.+?)\](?<text>.+?)\[/url\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeUrlWithParamRegex();

    [GeneratedRegex(@"\[img\](?<url>.+?)\[/img\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeImgRegex();

    [GeneratedRegex(@"\[c\](?<text>.+?)\[/c\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeCodeRegex();

    [GeneratedRegex(@"\[code\](?<text>.+?)\[/code\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeCodeBlockRegex();

    [GeneratedRegex(@"\[b\](?<text>.+?)\[/b\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeBoldRegex();

    [GeneratedRegex(@"\[i\](?<text>.+?)\[/i\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeItalicRegex();

    [GeneratedRegex(@"\[s\](?<text>.+?)\[/s\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeStrikethroughRegex();

    [GeneratedRegex(@"\[u\](?<text>.+?)\[/u\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeUnderlineRegex();

    [GeneratedRegex(@"\[imagemap\](\s*(?<url>\S+)[^\[]+?)\[/imagemap\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeImageMapRegex();

    [GeneratedRegex(@"\[heading\](?<text>.+?)\[/heading\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeHeadingRegex();

    [GeneratedRegex(@"\[size=(\d+?)\](?<text>.+?)\[/size\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeSizeRegex();

    [GeneratedRegex(@"\[notice\](?<text>.+?)\[/notice\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeNoticeRegex();

    [GeneratedRegex(@"\[quote(=.+?)?\](?<text>.+?)\[/quote\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeQuoteRegex();

    [GeneratedRegex(@"\[quote(=.+?)?\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeQuoteStartRegex();

    [GeneratedRegex(@"\[/quote\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeQuoteEndRegex();

    [GeneratedRegex("^", RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex LineStartRegex();

    public static string BbCodeToDiscordMd(string bbCode)
    {
        // Step 1: (Pre-processing) Remove nested quotes to avoid long quotes using up all the characters.

        var nestingLevel = 0;
        var lines = bbCode.Split("\n").ToList();
        var removedLines = 0;

        foreach (var (line, i) in lines.ToArray().WithIndexes())
        {
            if (BbCodeQuoteEndRegex().IsMatch(line))
            {
                RemoveIfNested();
                nestingLevel--;
            }
            else
            {
                if (BbCodeQuoteStartRegex().IsMatch(line))
                {
                    nestingLevel++;
                }

                RemoveIfNested();
            }

            continue;

            void RemoveIfNested()
            {
                if (nestingLevel > 1)
                {
                    lines.RemoveAt(i - removedLines++);
                }
            }
        }

        // Step 2: Escape special characters that have meaning in Discord Markdown.

        lines = lines.Select(line =>
        {
            if (line.StartsWith('#'))
            {
                line = line.Replace("#", @"\#");
            }
            else if (line.StartsWith('>'))
            {
                line = LineStartRegex().Replace(line, "> ");
            }

            return line;
        }).ToList();

        var result = string.Join("\n", lines);

        result = result.Replace("*", @"\*")
            .Replace("_", @"\_")
            .Replace("~", @"\~")
            .Replace("`", @"\`");

        // Step 3: Replace BBCode with Discord Markdown equivalents.

        result = BbCodeSimpleUrlRegex().Replace(result, "${url}");
        result = BbCodeUrlWithParamRegex().Replace(result, "[${text}](${url})");
        result = BbCodeImgRegex().Replace(result, "[*Image*](${url})");
        result = BbCodeCodeRegex().Replace(result, "`${text}`");
        result = BbCodeCodeBlockRegex().Replace(result, "```${text}```");
        result = BbCodeBoldRegex().Replace(result, "**${text}**");
        result = BbCodeItalicRegex().Replace(result, "*${text}*");
        result = BbCodeStrikethroughRegex().Replace(result, "~~${text}~~");
        result = BbCodeUnderlineRegex().Replace(result, "__${text}__");
        result = BbCodeImageMapRegex().Replace(result, "[*Image Map*](${url})");
        result = BbCodeHeadingRegex().Replace(result, "## ${text}");
        result = BbCodeSizeRegex().Replace(result, "${text}");
        result = BbCodeNoticeRegex().Replace(result, "${text}");
        result = BbCodeQuoteRegex().Replace(result, m =>
            LineStartRegex().Replace(m.Groups["text"].Value, "> ")
        );

        return result;
    }
}
