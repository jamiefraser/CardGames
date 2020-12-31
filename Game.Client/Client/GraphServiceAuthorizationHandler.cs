using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Game.Client.Client
{
    public class GraphServiceAuthorizationHandler : AuthorizationMessageHandler
    {
        public GraphServiceAuthorizationHandler(IAccessTokenProvider provider, NavigationManager navigationManager, IConfiguration config) : base(provider, navigationManager)
        {
            var graphBase = config["GraphServiceRoot"];
            ConfigureHandler(
                authorizedUrls: new[] { $"{graphBase}" },
                scopes: new[] { "https://gameroomsdev.onmicrosoft.com/api/Access.API" });
        }
    }
}
