// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using NsfwSpyNS;
using Numerous.Bot.Database;
using Numerous.Bot.DependencyInjection;

namespace Numerous.Bot.Discord.Services;

[HostedService]
public sealed class NuMod(DiscordSocketClient client, INsfwSpy nsfwSpy, IDbService db) : IHostedService
{
    private const float DeleteThreshold = 0.4f;
    private const float ReportThreshold = 0.9f;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.MessageReceived += message =>
        {
            _ = Task.Run(async () =>
            {
                if (message.Author == client.CurrentUser || message.Channel is not IGuildChannel channel)
                {
                    return;
                }

                var guildOptions = await db.GuildOptions.FindByIdAsync(channel.GuildId, cancellationToken);

                if ((channel as ITextChannel)?.IsNsfw == true
                    || guildOptions is not { NuModEnabled: true }
                    || (guildOptions.AdminsBypassNuMod && ((SocketGuildUser)message.Author).GuildPermissions.Administrator))
                {
                    return;
                }

                var imageAttachments = message
                    .Attachments
                    .Where(x => x.ContentType.StartsWith("image/"));

                var tasks = imageAttachments.Select(async attachment =>
                {
                    var uri = new Uri(attachment.Url);
                    var imageType = attachment.ContentType.Split('/')[1];

                    if (imageType == "gif")
                    {
                        var result = await nsfwSpy.ClassifyGifAsync(uri);

                        return result.Frames.Select(x => x.Value.Neutral).Min();
                    }
                    else
                    {
                        var result = await nsfwSpy.ClassifyImageAsync(uri);

                        return result.Neutral;
                    }
                });

                var neutral = (await Task.WhenAll(tasks)).Min();

                if (neutral >= ReportThreshold)
                {
                    return;
                }

                var logChannelId = (await db.GuildOptions.FindByIdAsync(channel.GuildId, cancellationToken))?.NuModReportChannel;

                if (logChannelId is null)
                {
                    return;
                }

                var logChannel = await channel.Guild.GetTextChannelAsync(logChannelId.Value);

                if (logChannel is null)
                {
                    return;
                }

                if (neutral < DeleteThreshold)
                {
                    await message.DeleteAsync();
                    await db.DiscordMessages.SetHiddenAsync(message.Id, cancellationToken: cancellationToken);

                    await logChannel.SendMessageAsync(
                        string.Join('\n', message.Attachments.Select(x => x.Url)),
                        embed: new EmbedBuilder()
                            .WithColor(Color.Red)
                            .WithTitle("NuMod - Report")
                            .WithDescription(
                                $"Message by {message.Author.Mention} in <#{channel.Id}> contains attachment with NSFW content ({((1 - neutral) * 100):0.00}%).\n"
                                + $"The message has been deleted."
                            ).Build());
                }
                else if (neutral < ReportThreshold)
                {
                    await logChannel.SendMessageAsync(
                        string.Join('\n', message.Attachments.Select(x => x.Url)),
                        embed: new EmbedBuilder()
                            .WithColor(Color.Orange)
                            .WithTitle("NuMod - Warning")
                            .WithDescription(
                                $"Message {message.GetLink()} by {message.Author.Mention} contains attachment with **potential** NSFW content ({((1 - neutral) * 100):0.00}%).\n"
                                + $"The message has **not** been deleted."
                            ).Build()
                    );
                }
            }, cancellationToken);

            return Task.CompletedTask;
        };

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
