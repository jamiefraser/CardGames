using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Game.Client.Client
{
    public class DeckServiceAuthorizationHandler : AuthorizationMessageHandler
    {
        public DeckServiceAuthorizationHandler(IAccessTokenProvider provider, NavigationManager navigationManager, IConfiguration config) : base(provider, navigationManager)
        {
            var deckBase = config["DeckServiceRoot"];
            ConfigureHandler(
                authorizedUrls: new[] { $"{deckBase}" },
                scopes: new[] { "https://gameroomsdev.onmicrosoft.com/api/Access.API" });
        }
        
    }
}
