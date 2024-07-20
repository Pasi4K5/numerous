using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Coravel;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Numerous.Bot;
using Numerous.Bot.Web.SauceNao;
using Numerous.Bot.Configuration;
using Numerous.Common.DependencyInjection;
using Numerous.Web.Auth;
using Numerous.Web.Components;
using Refit;
using Serilog;
using Serilog.Exceptions;
using DiHelper = Numerous.Web.DependencyInjection.DiHelper;

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
        ).Enrich.WithExceptionDetails()
        .MinimumLevel.Debug();

    Log.Logger = logCfg.CreateLogger();

    loggersInitialized = true;

    Log.Logger.Information(copyrightNotice);

    #endregion

    #region Services

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    var services = builder.Services;

    foreach (var serviceTuple in DiHelper.GetServices())
    {
        switch (serviceTuple.serviceType)
        {
            case ServiceType.Singleton:
                services.AddSingleton(serviceTuple.type, serviceTuple.impl);

                break;
            case ServiceType.Hosted:
                typeof(ServiceCollectionHostedServiceExtensions).GetMethods()
                    .First(m => m is
                        {
                            Name: nameof(ServiceCollectionHostedServiceExtensions.AddHostedService),
                            IsGenericMethodDefinition: true,
                        }
                    ).MakeGenericMethod(serviceTuple.impl)
                    .Invoke(services, [services]);

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceTuple.serviceType));
        }
    }

    var discordClient = new DiscordSocketClient(new()
    {
        GatewayIntents = GatewayIntents.All,
    });

    var cfgService = new ConfigService();
    var cfg = cfgService.Get();

    services.AddSingleton(discordClient);
    services.AddSingleton<IConfigService>(cfgService);
    services.AddSingleton<InteractionService>();
    services.AddScheduler();
    services.AddControllers();
    services.AddHttpClient();
    services.AddHttpContextAccessor();
    services.Configure<ForwardedHeadersOptions>(opt =>
    {
        opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });
    services.AddAuthentication(opt =>
        {
            opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = DiscordConstants.AuthenticationScheme;
        })
        .AddCookie(opt =>
        {
            opt.ExpireTimeSpan = TimeSpan.FromMinutes(1);
        })
        .AddOAuth(DiscordConstants.AuthenticationScheme, opt =>
        {
            opt.ClientId = cfg.DiscordClientId.ToString();
            opt.ClientSecret = cfg.DiscordClientSecret;
            opt.CallbackPath = "/redirect/discord";
            opt.AuthorizationEndpoint = "https://discordapp.com/api/oauth2/authorize";
            opt.TokenEndpoint = "https://discordapp.com/api/oauth2/token";
            opt.UserInformationEndpoint = "https://discordapp.com/api/users/@me";
            opt.Scope.Add("identify");
            opt.SaveTokens = true;
            opt.UsePkce = true;
            opt.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            opt.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");

            opt.Events.OnCreatingTicket = async context =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);

                response.EnsureSuccessStatusCode();

                var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

                context.RunClaimActions(user);
            };
        });
    services.AddCascadingAuthenticationState();
    services.Configure<HttpsRedirectionOptions>(opt =>
    {
        opt.HttpsPort = 443;
    });

    Bot.RegisterServices(services);

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseRouting();
    app.MapDefaultControllerRoute();
    app.UseStaticFiles();
    app.UseAuthorization();
    app.UseAntiforgery();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();

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
