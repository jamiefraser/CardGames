using Game.Client.Shared.Services.CurrentUser;
using Game.Client.Shared.Services.SignalRService;
using Game.Client.Shared.ViewModels;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using StateManager;
using StateManager.Extensions;
using Syncfusion.Blazor;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Game.Client.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzc1ODc0QDMxMzgyZTM0MmUzMEY2aHNQY08reVMxd2FTOWVyeEdBOHZMR1N1V1lqdHhjT1d2TjBIWWt3N289");

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            var gameServiceRoot = builder.Configuration["GameServiceRoot"];
            var deckServiceRoot = builder.Configuration["DeckServiceRoot"];
            var tableServiceRoot = builder.Configuration["TableServiceRoot"];
            var graphServiceRoot = builder.Configuration["GraphServiceRoot"];
            var presenceServiceRoot = builder.Configuration["PresenceServiceRoot"];
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddSingleton<ICurrentUserService>(new CurrentUserService());
            builder.Services.AddSingleton<ISignalRService>(sp => new SignalRService(sp.GetRequiredService<IHttpClientFactory>(), presenceServiceRoot, sp.GetRequiredService<ICurrentUserService>()));

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore(options =>
            {
            });


            #region HttpClientFactories for REST Services Service with authentication and error handling policies
            var httpTransientErrorRetryPolicy = Polly.Extensions.Http.HttpPolicyExtensions.HandleTransientHttpError().RetryAsync(3);
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(90);
            var writeFallBackPolicy = Policy.Handle<HttpRequestException>().Fallback(async (token) =>
            {
                System.Diagnostics.Debug.WriteLine($"Falling back from a write!!");

            });

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
                options.ProviderOptions.DefaultAccessTokenScopes.Add("https://gameroomsdev.onmicrosoft.com/api/Access.API");
                options.ProviderOptions.Cache.CacheLocation = "sessionStorage";
            });

            //Now create a named service instances for HttpClients that points to the correct base addresses (based on your handler configuration) and 
            //instantiates the right authorization handler for that client
            #region GameAPI
            builder.Services.AddScoped<GameServiceAuthorizationHandler>();

            builder.Services.AddHttpClient("gameAPI",
                client =>
                {
                    client.BaseAddress = new Uri($"{gameServiceRoot}");
                })
                //The AuthorizationHandler ensures an refreshed API access token is attached to every request to the URI specified in the BaseAddress
                .AddHttpMessageHandler<GameServiceAuthorizationHandler>()
                //Tack the Polly policies we created above onto the HttpClientFactory
                //So every HttpClient constructed by the factory follows a consistent set of rules for dealing with error conditions
                //You can always tack more policies onto specific instances to deal with your specific condition as well
                //ProTip:  I believe this would make for a lovely solution to logging certain classes of issues which would lead to better manageability
                .AddPolicyHandler(httpTransientErrorRetryPolicy)
                .AddPolicyHandler(timeoutPolicy);

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient("gameAPI"));
            #endregion

            #region DeckAPI
            builder.Services.AddScoped<DeckServiceAuthorizationHandler>();

            builder.Services.AddHttpClient("deckAPI",
                client =>
                {
                    client.BaseAddress = new Uri($"{deckServiceRoot}");
                })
                .AddHttpMessageHandler<DeckServiceAuthorizationHandler>()
                .AddPolicyHandler(httpTransientErrorRetryPolicy)
                .AddPolicyHandler(timeoutPolicy);

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
               .CreateClient("deckAPI"));
            #endregion

            #region TableAPI
            builder.Services.AddScoped<TableServiceAuthorizationHandler>();

            builder.Services.AddHttpClient("tableAPI",
                client =>
                {
                    client.BaseAddress = new Uri($"{tableServiceRoot}");
                })
                .AddHttpMessageHandler<TableServiceAuthorizationHandler>()
                .AddPolicyHandler(httpTransientErrorRetryPolicy)
                .AddPolicyHandler(timeoutPolicy);

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
               .CreateClient("tableAPI"));
            #endregion

            #region GraphAPI
            builder.Services.AddScoped<GraphServiceAuthorizationHandler>();

            builder.Services.AddHttpClient("graphAPI",
                client =>
                {
                    client.BaseAddress = new Uri($"{graphServiceRoot}");
                })
                .AddHttpMessageHandler<GraphServiceAuthorizationHandler>()
                .AddPolicyHandler(httpTransientErrorRetryPolicy)
                .AddPolicyHandler(timeoutPolicy);

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
               .CreateClient("graphAPI"));
            #endregion

            #region PresenceAPI
            builder.Services.AddScoped<GraphServiceAuthorizationHandler>();

            builder.Services.AddHttpClient("presenceAPI",
                client =>
                {
                    client.BaseAddress = new Uri($"{presenceServiceRoot}");
                })
                .AddPolicyHandler(httpTransientErrorRetryPolicy)
                .AddPolicyHandler(timeoutPolicy);

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
               .CreateClient("presenceAPI"));
            #endregion
            #endregion

            #region Register ViewModels
            builder.Services.AddTransient<IStartAGameViewModel, StartAGameViewModel>();
            builder.Services.AddTransient<ILobbyViewModel, LobbyViewModel>();
            #endregion
            #region State Manager - wiring in for presence detection
            builder.Services.AddSyncfusionBlazor();
            builder.Services.AddTransient(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddStateManagemenet();
            var built = builder.Build();
            await InitializeState(built.Services,
               "stateSaved", "There are unsaved changes. Quit anyway?");
            await built.Services.EnableUnloadEvents();

            await built.RunAsync();
            #endregion
        }

        private async static Task InitializeState(IServiceProvider services, string errorStateKey,string exitConfirm)
        {
            var state = services.GetRequiredService<TasksStateService>();
            state.ErrorKey = errorStateKey;
            state.UnloadKey = errorStateKey;
            state.UnloadPrompt = exitConfirm;
            if (await state.Load(errorStateKey))
            {
                await state.Delete(errorStateKey);
            }
            state.BeforeUnload += () =>
            {
                return Helpers.UpdateStatus(services.GetRequiredService<ICurrentUserService>().CurrentClaimsPrincipal, services.GetRequiredService<IHttpClientFactory>(), false);
            };

        }
    }
}
