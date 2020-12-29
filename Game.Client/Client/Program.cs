using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Extensions;
namespace Game.Client.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            var gameServiceRoot = builder.Configuration["GameServiceRoot"];
            var deckServiceRoot = builder.Configuration["DeckServiceRoot"];
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore(options =>
            {

            });
            builder.Services.AddScoped<GameServiceAuthorizationHandler>();
            builder.Services.AddScoped<DeckServiceAuthorizationHandler>();

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
            });

            //Now create a named service instances for HttpClients that points to the correct base addresses (based on your handler configuration) and 
            //instantiates the right authorization handler for that client
            #region GameAPI
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
            #endregion
            await builder.Build().RunAsync();
        }
    }
}
