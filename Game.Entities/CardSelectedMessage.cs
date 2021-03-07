using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class CardSelectedMessage
    {
        public Entities.Player Player { get; set; }
        public int CardIndex { get; set; }
    }
}
