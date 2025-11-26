// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.DependencyInjection;
using Numerous.Bot.Discord.Events;
using Numerous.Bot.Discord.Interactions;
using Numerous.Bot.Discord.Services;
using Numerous.Bot.Discord.Services.Attachments;
using Numerous.Bot.Discord.Util;
using Numerous.Bot.Exclusive;
using Numerous.Bot.Osu;
using Numerous.Bot.Services;
using Numerous.Bot.Util;
using Numerous.Bot.Web;
using Numerous.Bot.Web.Osu;
using Numerous.Bot.Web.SauceNao;
using Numerous.DiscordAdapter.DiscordDotNet.Util;
using Refit;

namespace Numerous.Bot;

public static class BotServiceConfiguration
{
    public static void Configure(IServiceCollection services)
    {
        services
            .AddDiscordDotNetAdapter()
            .AddSingleton<AttachmentService>()
            .AddTransient<BeatmapService>()
            .AddSingleton<CommandFinder>()
            .AddSingleton<DiscordEventHandler>()
            .AddTransient<EmbedBuilders>()
            .AddSingleton<IFileService, FileService>()
            .AddTransient<GuildStatsService>()
            .AddTransient<HttpLoggingHandler>()
            .AddHostedService<InteractionHandler>()
            .AddHostedService<MapFeedService>()
            .AddHostedService<MessageResponder>()
            .AddHostedService<MudaeMessageHandler>()
            .AddSingleton<IOsuApiRepository, OsuApiRepository>()
            .AddHostedService<OsuForumService>()
            .AddTransient<OsuHttpHandler>()
            .AddSingleton<OsuUserStatsService>()
            .AddSingleton<IOsuVerifier, OsuVerifier>()
            .AddSingleton<IOsuTokenProvider, OsuTokenProvider>()
            .AddSingleton<ReminderService>()
            .AddSingleton<ISauceNaoClient, SauceNaoClient>()
            .AddTransient<ScoreValidator>()
            .AddHostedService<StarReactPreventionService>()
            .AddHostedService<Startup>()
            .AddSingleton<VerificationService>();

        // TODO: Consider switching to System.Text.Json (for everything)
        services
            .AddRefitClient<ISauceNaoApi>(new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer(),
            })
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(ISauceNaoApi.BaseUrl);
            })
            .AddHttpMessageHandler<HttpLoggingHandler>();

        services.AddRefitClient<IOsuApi>(new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer(),
            })
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(IOsuApi.BaseUrl);
            })
            .AddHttpMessageHandler<OsuHttpHandler>();
    }
}
