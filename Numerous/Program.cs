// Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Numerous.Configuration;
using Numerous.DependencyInjection;
using Serilog;
using Serilog.Exceptions;

const string copyrightNotice =
    """
    Copyright (C) Pasi4K5 <https://www.github.com/Pasi4K5>
    This program comes with ABSOLUTELY NO WARRANTY.
    This is free software, and you are welcome to redistribute it under certain conditions.
    See <https://www.gnu.org/licenses/gpl-3.0> for details.
    """;

var loggersInitialized = false;

try
{
    var cm = new ConfigManager();

    #region Logger

    var logCfg = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File(
            $"./logs/log{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff_}.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: null,
            retainedFileTimeLimit: null,
            fileSizeLimitBytes: null,
            buffered: true
        ).Enrich.WithExceptionDetails();

    if (cm.Get().DevMode)
    {
        logCfg.MinimumLevel.Debug();
    }
    else
    {
        logCfg.MinimumLevel.Information();
    }

    Log.Logger = logCfg.CreateLogger();

    loggersInitialized = true;

    Log.Logger.Information(copyrightNotice);

    #endregion

    #region Services

    var services = Host.CreateDefaultBuilder();

    services.ConfigureServices((_, s) =>
    {
        foreach (var serviceTuple in DiHelper.GetServices())
        {
            switch (serviceTuple.serviceType)
            {
                case ServiceType.Singleton:
                    s.AddSingleton(serviceTuple.type, serviceTuple.impl);

                    break;
                case ServiceType.Hosted:
                    typeof(ServiceCollectionHostedServiceExtensions).GetMethods()
                        .First(m => m is
                            {
                                Name: nameof(ServiceCollectionHostedServiceExtensions.AddHostedService),
                                IsGenericMethodDefinition: true,
                            }
                        ).MakeGenericMethod(serviceTuple.impl)
                        .Invoke(s, new object[] { s });

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceTuple.serviceType));
            }
        }

        var client = new DiscordSocketClient(new()
        {
            GatewayIntents = GatewayIntents.All,
        });

        s.AddSingleton(client);
        s.AddSingleton(cm);
        s.AddSingleton<InteractionService>();
    });

    using var host = services.Build();

    await host.RunAsync();

    #endregion
}
catch (Exception e)
{
    if (loggersInitialized)
    {
        Log.Error(e, "An unhandled exception occurred");
    }
    else
    {
        Console.WriteLine(e);
    }
}
finally
{
    await Log.CloseAndFlushAsync();
}
