using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class PlayerHandMessage
    {
        public List<Card> Hand { get; set; }
        public Guid TableId { get; set; }
    }
}
