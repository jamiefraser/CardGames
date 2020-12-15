using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class EasyAuthUserInfo
    {
        public string PrincipalName { get; set; }
        public string PrincipalId { get; set; }
        public string PrincipalIdp { get; set; }
        public Dictionary<string, string> ZumoHeaders { get; set; }
    }
}
