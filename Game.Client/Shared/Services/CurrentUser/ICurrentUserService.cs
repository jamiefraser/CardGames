using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Game.Client.Shared.Services.CurrentUser
{
    public interface ICurrentUserService
    {
        System.Security.Claims.ClaimsPrincipal CurrentClaimsPrincipal { get; set; }
        string CurrentClaimsPrincipalOid { get; set; }
    }
}
