using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class Player : TableEntity
    {
        public string PrincipalName { get; set; }
        public string PrincipalId { get; set; }
        public string PrincipalIdp { get; set; }
        public List<Card> Hand { get; set; }
        public EasyAuthUserInfo ToEasyAuthUserInfo()
        {
            return new EasyAuthUserInfo()
            {
                PrincipalId = this.PrincipalId,
                PrincipalIdp = this.PrincipalIdp,
                PrincipalName = this.PrincipalName
            };
        }

    }
}
