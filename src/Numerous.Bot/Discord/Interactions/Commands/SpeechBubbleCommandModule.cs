// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Reflection;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Numerous.Bot.Discord.Interactions.Commands;

public sealed class SpeechBubbleCommandModule(IHttpClientFactory clientFactory) : InteractionModule
{
    [UsedImplicitly]
    [SlashCommand("speechbubble", "Generates a speech bubble meme with the given image")]
    public async Task SpeechBubble(
        [Summary("image", "Source image")]
        IAttachment attachment
    )
    {
        if (!attachment.ContentType.StartsWith("image/"))
        {
            await RespondWithEmbedAsync(
                message: "The file you uploaded is not an image.",
                type: ResponseType.Error
            );

            return;
        }

        var speechBubbleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Numerous.Bot.Resources.speech_bubble.png");

        if (speechBubbleStream is null)
        {
            throw new FileNotFoundException("Could not find 'speech_bubble.png'.");
        }

        await DeferAsync();

        using var speechBubble = await Image.LoadAsync<Rgba32>(speechBubbleStream);

        using var client = clientFactory.CreateClient();
        await using var customImageStream = await client.GetStreamAsync(attachment.Url);

        try
        {
            var customImage = await Image.LoadAsync<Rgba32>(customImageStream);

            var customImageAspectRatio = (float)customImage.Width / customImage.Height;
            var speechBubbleAspectRatio = (float)speechBubble.Width / speechBubble.Height;
            speechBubble.Mutate(x => x.Resize(new Size(
                customImage.Width,
                (int)Math.Floor((double)customImage.Width / speechBubbleAspectRatio / customImageAspectRatio)))
            );

            using var result = new Image<Rgba32>(customImage.Width, customImage.Height);

            for (var y = 0; y < customImage.Height; y++)
            {
                for (var x = 0; x < customImage.Width; x++)
                {
                    if (y < speechBubble.Height)
                    {
                        var imgPixel = customImage[x, y];
                        var maskPixel = speechBubble[x, y];
                        var alpha = (double)maskPixel.R / 255;
                        result[x, y] = new Rgba32(imgPixel.R, imgPixel.G, imgPixel.B, (byte)Math.Round(alpha * imgPixel.A));
                    }
                    else
                    {
                        result[x, y] = customImage[x, y];
                    }
                }
            }

            using var resultStream = new MemoryStream();
            await result.SaveAsPngAsync(resultStream);
            await FollowupWithFileAsync(resultStream, "speech_bubble.png");
        }
        catch (UnknownImageFormatException)
        {
            await FollowupWithEmbedAsync(
                message: "This image format is not supported.",
                type: ResponseType.Error
            );
        }
    }
}
