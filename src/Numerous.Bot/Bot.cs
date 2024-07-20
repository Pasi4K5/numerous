// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.DependencyInjection;
using Numerous.Bot.Web.Osu;
using Numerous.Bot.Web.SauceNao;
using Refit;

namespace Numerous.Bot;

public static class Bot
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // TODO: Consider switching to System.Text.Json (for everything)
        services
            .AddRefitClient<ISauceNaoApi>(new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer(),
            })
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(ISauceNaoApi.BaseUrl);
            });

        services.AddRefitClient<IOsuApi>(new RefitSettings
            {
                ContentSerializer = new NewtonsoftJsonContentSerializer(),
            })
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(IOsuApi.BaseUrl);
            })
            .AddHttpMessageHandler<OsuAuthHandler>();
    }
}
