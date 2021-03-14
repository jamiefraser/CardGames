using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class TableStartedMessage
    {
        public string TableId { get; set; }
        public Player Dealer { get; set; }
    }
}
