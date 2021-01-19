using Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared
{
    public static class Helpers
    {
        public static Entities.Player ToPlayer(this ClaimsPrincipal principal)
        {
            var player = new Entities.Player()
            {
                Hand = new List<Card>(),
                PrincipalId = principal.Claims.Where(c => c.Type == "oid").FirstOrDefault().Value,
                PrincipalName = principal.Identities.FirstOrDefault().Name,
                PrincipalIdp = principal.Identities.FirstOrDefault().AuthenticationType
            };
            return player;
        }
    }
}
