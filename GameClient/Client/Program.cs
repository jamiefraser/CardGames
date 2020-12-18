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
using Microsoft.Extensions.Configuration;
//using Toolbelt.Blazor.Extensions.DependencyInjection; 
namespace Game.Client.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            //builder.Configuration.AddJsonFile("/appsettings.json");
            builder.RootComponents.Add<App>("app");
            //builder.Services.AddHttpClientInterceptor();
            var gameServiceRoot = builder.Configuration.GetValue<string>("GameServiceRoot");
            builder.Services.AddHttpClient("Game.Client.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
            builder.Services.AddHttpClient("GameService", client => client.BaseAddress = new Uri(gameServiceRoot))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server projecte
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Game.Client.ServerAPI"));

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
                options.ProviderOptions.DefaultAccessTokenScopes.Add("https://gameroomsdev.onmicrosoft.com/api/Access.API");
            });

            await builder.Build().RunAsync();
        }
    }
}
