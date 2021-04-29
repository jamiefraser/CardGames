using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class RequestToJoinTableMessage
    {
        public Entities.Table Table { get; set; }
        public Entities.Player RequestingPlayer { get; set; }
        public string TableOwnerId { get; set; }
    }
}
