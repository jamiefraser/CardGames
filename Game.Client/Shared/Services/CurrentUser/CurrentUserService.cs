using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Game.Client.Shared.Services.CurrentUser
{
    public class CurrentUserService : ICurrentUserService
    {
        private ClaimsPrincipal currentClaimsPrincipal;
        public ClaimsPrincipal CurrentClaimsPrincipal
        {
            get
            {
                return currentClaimsPrincipal;
            }
            set
            {
                currentClaimsPrincipal = value;
            }
        }
        private string currentClaimsPrincipalOid;
        public string CurrentClaimsPrincipalOid
        {
            get
            {
                return currentClaimsPrincipalOid;
            }
            set
            {
                currentClaimsPrincipalOid = value;
            }
        }
        private ClaimsPrincipal signingoutclaimsprincipal;
        public ClaimsPrincipal SigningOutClaimsPrincipal
        {
            get
            {
                return signingoutclaimsprincipal;
            }
            set
            {
                signingoutclaimsprincipal = value;
            }
        }
    }
}
