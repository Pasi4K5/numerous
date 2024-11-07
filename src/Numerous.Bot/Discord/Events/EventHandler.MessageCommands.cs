// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Net;
using Discord;
using Numerous.Bot.Util;
using Refit;

namespace Numerous.Bot.Discord.Events;

public partial class DiscordEventHandler
{
    [Init]
    private void MessageCommands_Init()
    {
        client.MessageReceived += MessageCommands_HandleAsync;
    }

    private async Task MessageCommands_HandleAsync(IMessage msg)
    {
        var cfg = cfgProvider.Get();

        if (msg.Author.IsBot || msg.Author.Id != cfg.OwnerDiscordId || !msg.Content.StartsWith(cfg.Prefix))
        {
            return;
        }

        var args = msg.Content[cfg.Prefix.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var command = args[0].ToLowerInvariant();
        args = args[1..];

        if (command == "manverify")
        {
            if (args.Length != 2)
            {
                await msg.ReplyAsync("Invalid number of arguments.");

                return;
            }

            if (!ulong.TryParse(args[0], out var discordId))
            {
                await msg.ReplyAsync("Invalid Discord user ID.");

                return;
            }

            var user = await client.GetUserAsync(discordId);

            if (user is null)
            {
                await msg.ReplyAsync("Discord user not found.");

                return;
            }

            var isVerified = await verifier.UserIsVerifiedAsync(user);

            if (isVerified)
            {
                await msg.ReplyAsync($"{user.Mention} is already verified.");

                return;
            }

            try
            {
                var osuUser = await osuApi.GetUserAsync(args[1]);

                await verifier.VerifyAsync(user, osuUser.Id);

                await msg.ReplyAsync(
                    $"{user.Mention} has been manually verified as *[{osuUser.Username}](https://osu.ppy.sh/users/{osuUser.Id})*."
                );
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                await msg.ReplyAsync("osu! user not found.");
            }
        }
    }
}
