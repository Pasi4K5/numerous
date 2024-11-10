// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Web.SauceNao;

namespace Numerous.Bot.Discord.Interactions.Commands;

public sealed class SauceCommandModule(ISauceNaoClient sauceNao) : InteractionModule
{
    // TODO: Rate limit needs to be handled.
    // Current rate limit is 4 per 30 seconds and 100 per 24 hours.
    [UsedImplicitly]
    [MessageCommand("Sauce")]
    [CommandContextType(InteractionContextType.Guild)]
    public async Task SauceCommand(IMessage msg)
    {
        var imgAttachments = msg.Attachments
            .Where(a => a.ContentType.StartsWith("image/"))
            .ToArray();

        if (imgAttachments.Length != 1)
        {
            await RespondWithEmbedAsync(
                message: "This command can only be used on messages with exactly one image attachment.",
                type: ResponseType.Error
            );

            return;
        }

        await DeferAsync();

        var attachment = imgAttachments.First();
        var response = await sauceNao.SearchAsync(attachment.Url);

        var result = response.Results.FirstOrDefault();
        var data = result.Data;
        var urls = data.ExtUrls;

        if (result == default || urls.Count == 0)
        {
            await FollowupWithEmbedAsync(
                message: "No results found.",
                type: ResponseType.Special
            );
        }
        else
        {
            var header = result.Header;

            var eb = new EmbedBuilder()
                .WithColor(GetTypeColor(ResponseType.Success))
                .WithTitle(data.Title ?? "Unknown Title")
                .WithDescription(data.MemberName ?? "Unknown Artist")
                .WithFields([
                    new()
                    {
                        Name = "Sources",
                        Value = string.Join('\n', urls.Select(url => $"[Source {urls.IndexOf(url) + 1}]({url})")),
                        IsInline = true,
                    },
                    new()
                    {
                        Name = "Similarity",
                        Value = header.Similarity + "%",
                        IsInline = true,
                    },
                ])
                .WithUrl(data.ExtUrls.First());

            if (!result.Header.Hidden)
            {
                eb.WithThumbnailUrl(result.Header.Thumbnail);
            }
            else
            {
                eb.WithFields([
                    new()
                    {
                        Name = "Warning!",
                        Value = "This image might contain explicit content. Proceed with caution.",
                    },
                ]);
            }

            await FollowupAsync(embed: eb.Build());
        }
    }
}
