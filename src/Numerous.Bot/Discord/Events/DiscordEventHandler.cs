// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord.WebSocket;
using Numerous.Bot.Util;
using Numerous.Bot.Web.Osu;
using Numerous.Common.Config;
using Numerous.Database.Context;

namespace Numerous.Bot.Discord.Events;

// TODO: This whole class fucking sucks.
public sealed partial class DiscordEventHandler(
    IConfigProvider cfgProvider,
    DiscordSocketClient client,
    IUnitOfWorkFactory uowFactory,
    AttachmentService attachmentService,
    OsuVerifier verifier,
    IOsuApiRepository osuApi
)
{
    public void Start()
    {
        this.Init();
    }
}
