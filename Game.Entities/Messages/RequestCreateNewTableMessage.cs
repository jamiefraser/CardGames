using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class RequestCreateNewTableMessage
    {
        public Table Table { get; set; }
        public Player Owner { get; set; }
    }
}
