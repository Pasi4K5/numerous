// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;

namespace Numerous.Bot.Util;

public static partial class MarkupTransformer
{
    [GeneratedRegex(@"\[url=(?<url>.+?)\](?<text>.+?)\[/url\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex BbCodeUrlRegex();

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

    // TODO: Temporarily disabled because it doesn't work on nested quotes
    // [GeneratedRegex(@"\[quote(=.+?)?\](?<text>.+?)\[/quote\]", RegexOptions.Compiled | RegexOptions.Singleline)]
    // private static partial Regex BbCodeQuoteRegex();

    public static string BbCodeToDiscordMd(string bbCode)
    {
        var result = bbCode;

        result = result.Replace("*", @"\*")
            .Replace("_", @"\_")
            .Replace("~", @"\~")
            .Replace("`", @"\`")
            .Replace("#", @"\#")
            .Replace(">", @"\>");

        result = BbCodeUrlRegex().Replace(result, "[${text}](${url})");
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

        // TODO: Temporarily disabled because it doesn't work on nested quotes
        // result = BbCodeQuoteRegex().Replace(result, m =>
        // {
        //     var quoteText = m.Groups["text"].Value;
        //     var quotedLines = LineStartRegex().Replace(quoteText, "> ");
        //
        //     return quotedLines;
        // });

        return result;
    }
}
