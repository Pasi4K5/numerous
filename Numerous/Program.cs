using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Numerous.Configuration;
using Numerous.DependencyInjection;
using Serilog;
using Serilog.Exceptions;

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
    Log.Error(e, "An unhandled exception occurred");
}
finally
{
    await Log.CloseAndFlushAsync();
}
