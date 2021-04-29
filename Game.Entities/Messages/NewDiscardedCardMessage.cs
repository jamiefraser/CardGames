using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class NewDiscardedCardMessage
    {
        public Entities.Card Card { get; set; }
        public Player NextPlayer { get; set; }
    }
}
