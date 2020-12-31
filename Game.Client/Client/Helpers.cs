using Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Game.Client.Client
{
    public static class Helpers
    {
        public static Player ToPlayer(this EasyAuthUserInfo user)
        {
            return new Player()
            {
                PrincipalId = user.PrincipalId,
                PrincipalIdp = user.PrincipalIdp,
                PrincipalName = user.PrincipalName
            };
        }
    }
}
