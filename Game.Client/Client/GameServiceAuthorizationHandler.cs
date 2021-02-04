using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Game.Client.Client
{
    public class GameServiceAuthorizationHandler : AuthorizationMessageHandler
    {
        public GameServiceAuthorizationHandler(IAccessTokenProvider provider,
                                               NavigationManager navigationManager, 
                                               IConfiguration config) : base(provider, navigationManager)
        {
            var gameBase = config["GameServiceRoot"];
            ConfigureHandler(
                authorizedUrls: new[] { $"{gameBase}" },
                scopes: new[] { "https://gameroomsdev.onmicrosoft.com/api/Access.API" });
        }
    }
}
