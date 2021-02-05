using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public interface IDeck
    {
        Guid Id { get; set; }
        Queue<Card> Cards { get; set; }
        DeckBase Shuffle();
    }
}
