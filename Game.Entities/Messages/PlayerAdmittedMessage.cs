using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Game.Entities
{
    public  class PlayerAdmittedMessage
    {
        public Player Player { get; set; }
        public Table Table { get; set; }
    }
}
