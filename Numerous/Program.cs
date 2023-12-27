using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Numerous.DependencyInjection;

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
                    .First(m => m is { Name: nameof(ServiceCollectionHostedServiceExtensions.AddHostedService), IsGenericMethodDefinition: true })
                    .MakeGenericMethod(serviceTuple.impl)
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
    s.AddSingleton<InteractionService>();
});

using var host = services.Build();

await host.RunAsync();
