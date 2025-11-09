// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Security.Cryptography;
using System.Text;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using Numerous.Bot.Discord.Services;
using Numerous.Bot.Discord.Util;
using Numerous.Common.Config;
using Numerous.Database.Context;
using SimpleCaptchaDotNet;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Numerous.Bot.Discord.Interactions.Commands.Config;

partial class ConfigCommandModule
{
    [Group("captcha", "Captcha commands")]
    public sealed class CaptchaGroup(IConfigProvider cfg, OsuVerifier verifier, IUnitOfWork uow) : InteractionModule
    {
        private const string ChooseCaptchaButtonId = "captcha:choose_captcha";
        private const string ChooseOsuButtonId = "captcha:choose_osu";
        private const string SolveCaptchaButtonId = "captcha:solve_captcha";
        private const string SolveCaptchaModalId = "captcha:solve_captcha_modal";
        private const string SolveCaptchaModalInputId = "captcha:solve_captcha_input";

        private const int CaptchaLength = 5;

        private readonly Emote _osuEmote = DiscordUtil.GetEmoteById(cfg.Get().Emojis.Osu);

        private static readonly CaptchaManager _captchaManager = new();
        private static readonly Crypto _crypto = new();

        private static readonly JpegEncoder _imgEncoder = new()
        {
            Quality = 50,
        };

        private static readonly Dictionary<ulong, DateTimeOffset> _lastCaptchaTimes = new();

        private static Dictionary<ulong, DateTimeOffset> LastCaptchaTimes
        {
            get
            {
                foreach (
                    var key in _lastCaptchaTimes
                        .Where(kvp => kvp.Value + _captchaCooldown < DateTimeOffset.UtcNow)
                        .Select(kvp => kvp.Key)
                )
                {
                    _lastCaptchaTimes.Remove(key);
                }

                return _lastCaptchaTimes;
            }
        }

        private static readonly TimeSpan _captchaCooldown = TimeSpan.FromSeconds(10);

        [UsedImplicitly]
        [SlashCommand("setup", "Set up CAPTCHA verification for new members.")]
        private async Task SetupCaptcha(
            [Summary("channel", "The channel where the CAPTCHA messages will be sent.")]
            ITextChannel channel,
            [Summary("title", "The title of the CAPTCHA message.")]
            string title = "✅ Verification Required",
            [Summary("description", "The description of the CAPTCHA message.")]
            string message =
                """
                **Welcome to the server!**

                To gain access to the other channels, please log in to your osu! account to prove you're not a bot.

                -# If you don't have an osu! account, you can solve a CAPTCHA instead.
                """,
            [Summary("log_channel", "The channel where verification logs will be sent.")]
            ITextChannel? logChannel = null
        )
        {
            if (
                channel.Guild.Id != Context.Guild.Id
                || (logChannel is not null && logChannel.Guild.Id != Context.Guild.Id)
            )
            {
                await RespondWithEmbedAsync(
                    title: "Invalid channel",
                    message: "The provided channel is not in this server.",
                    ResponseType.Error
                );

                return;
            }

            var msg = await channel.SendMessageAsync(
                embed: CreateEmbed(title, message).Build(),
                components: new ComponentBuilder()
                    .WithButton("Solve CAPTCHA", ChooseCaptchaButtonId, ButtonStyle.Secondary)
                    .WithButton("Login with osu!", ChooseOsuButtonId, ButtonStyle.Success, _osuEmote)
                    .Build()
            );

            await RespondWithEmbedAsync(message: $"Captcha message sent: {msg.GetJumpUrl()}");

            if (logChannel is not null)
            {
                await uow.Guilds.SetUserLogChannel(Context.Guild.Id, logChannel.Id);
                await uow.CommitAsync();
            }
        }

        [UsedImplicitly]
        [ComponentInteraction(ChooseCaptchaButtonId, true)]
        private async Task ChooseCaptcha()
        {
            if (!await CanInteract())
            {
                await DeferAsync();

                return;
            }

            if (LastCaptchaTimes.TryGetValue(Context.User.Id, out var lastTime))
            {
                var timeSinceLast = DateTimeOffset.UtcNow - lastTime;

                if (timeSinceLast < _captchaCooldown)
                {
                    var timeLeft = _captchaCooldown - timeSinceLast;
                    await RespondWithEmbedAsync(
                        title: "Cooldown",
                        message: $"Please wait {timeLeft.Seconds} more seconds before requesting a new CAPTCHA.\n",
                        type: ResponseType.Warning,
                        ephemeral: true
                    );

                    return;
                }
            }

            var respondTask = RespondAsync("**Generating CAPTCHA...**", ephemeral: true);

            var captcha = _captchaManager.Current;

            await using var stream = new MemoryStream();
            await captcha.Image.SaveAsync(stream, _imgEncoder);

            var (cipherText, iv) = _crypto.Encrypt(captcha.Text);

            await respondTask;

            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = "";
                msg.Attachments = new[]
                {
                    new FileAttachment(stream, "captcha.png"),
                };
                msg.Components = new ComponentBuilder()
                    .WithButton("Solve", $"{SolveCaptchaButtonId}:{cipherText},{iv}", emote: Emoji.Parse(":white_check_mark:"))
                    .Build();
            });

            _captchaManager.Next();

            LastCaptchaTimes[Context.User.Id] = DateTimeOffset.UtcNow;
        }

        [UsedImplicitly]
        [ComponentInteraction($"{SolveCaptchaButtonId}:*,*", true)]
        private async Task SolveCaptcha(string cipherText, string iv)
        {
            if (!await CanInteract())
            {
                await DeferAsync();

                return;
            }

            await RespondWithModalAsync<CaptchaModal>($"{SolveCaptchaModalId}:{cipherText},{iv}");
        }

        [UsedImplicitly]
        [ModalInteraction($"{SolveCaptchaModalId}:*,*", true)]
        private async Task SolveCaptchaModal(string cipherText, string iv, CaptchaModal modal)
        {
            var correctAnswer = _crypto.Decrypt(cipherText, iv);
            var answerIsCorrect = correctAnswer.Equals(modal.Input, StringComparison.OrdinalIgnoreCase);
            await DeferAsync();

            if (!answerIsCorrect)
            {
                await ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = "**:x: Wrong answer! Please try again with a different CAPTCHA!**";
                    msg.Attachments = Array.Empty<FileAttachment>();
                    msg.Components = new ComponentBuilder()
                        .WithButton("Retry", ChooseCaptchaButtonId, emote: Emoji.Parse(":repeat:"))
                        .Build();
                });

                return;
            }

            var guild = await uow.Guilds.GetAsync(Context.Guild.Id);
            var roleId = guild.VerifiedRoleId;
            var guildUser = (IGuildUser)Context.User;

            if (roleId is null)
            {
                await FollowupAsync("Verified role not configured.");

                return;
            }

            await guildUser.AddRoleAsync(roleId.Value);

            var dm = await guildUser.CreateDMChannelAsync();

            await dm.SendMessageAsync(
                embed: CreateEmbed(
                    "✅ Verification Successful",
                    $"**Welcome to {Context.Guild.Name}!**\n"
                    + $"If you want to link your osu! account later, use the `/link` command."
                ).Build()
            );
        }

        [UsedImplicitly]
        [ComponentInteraction(ChooseOsuButtonId, true)]
        private async Task ChooseOsu()
        {
            if (await verifier.UserIsVerifiedAsync(Context.User))
            {
                await DeferAsync();

                return;
            }

            await RespondAsync(
                components: new ComponentBuilder()
                    .WithButton(
                        "Click here to log in with osu!",
                        style: ButtonStyle.Link,
                        url: cfg.Get().BaseUrl,
                        emote: _osuEmote
                    ).Build(),
                ephemeral: true
            );
        }

        private async Task<bool> CanInteract()
        {
            var guild = await uow.Guilds.GetAsync(Context.Guild.Id);
            var guildUser = (IGuildUser)Context.User;

            return !await verifier.UserIsVerifiedAsync(guildUser)
                   && (guild.VerifiedRoleId is null || !guildUser.RoleIds.Contains(guild.VerifiedRoleId.Value));
        }

        private sealed class CaptchaModal : IModal
        {
            public string Title => "CAPTCHA";

            [UsedImplicitly]
            [RequiredInput]
            [InputLabel("Answer")]
            [ModalTextInput(
                SolveCaptchaModalInputId,
                TextInputStyle.Short,
                "Enter the text you see in the image.",
                minLength: CaptchaLength,
                maxLength: CaptchaLength
            )]
            public required string Input { get; set; }
        }

        private sealed class CaptchaManager
        {
            private readonly CaptchaFactory _captchaFactory = new(new()
            {
                BlurAmount = 3,
            }, new CaptchaPhraseFactory
            {
                Length = CaptchaLength,
                Characters = "ABCDEFGHIJKLMNPQRSTUVWXYZ23456789",
            });

            private Captcha _nextCaptcha;

            public Captcha Current => _nextCaptcha;

            public CaptchaManager()
            {
                _nextCaptcha = _captchaFactory.Next();
            }

            public void Next()
            {
                var oldCaptcha = _nextCaptcha;
                _nextCaptcha = _captchaFactory.Next();
                oldCaptcha.Dispose();
            }
        }

        private sealed class Crypto
        {
            private const int KeySize = 128 / 8;

            private readonly Aes _aes = Aes.Create();

            public Crypto()
            {
                _aes.Key = RandomNumberGenerator.GetBytes(KeySize);
            }

            public (string cipherText, string iv) Encrypt(string text)
            {
                _aes.GenerateIV();
                var plainText = Encoding.ASCII.GetBytes(text);
                var cipherText = _aes.EncryptCbc(plainText, _aes.IV);

                return (Convert.ToBase64String(cipherText), Convert.ToBase64String(_aes.IV));
            }

            public string Decrypt(string cipherText, string iv)
            {
                var cipherBytes = Convert.FromBase64String(cipherText);
                var ivBytes = Convert.FromBase64String(iv);
                var plainText = _aes.DecryptCbc(cipherBytes, ivBytes);

                return Encoding.ASCII.GetString(plainText);
            }
        }
    }
}
