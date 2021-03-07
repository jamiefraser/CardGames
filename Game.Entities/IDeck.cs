using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public interface IDeck
    {
        Guid Id { get; set; }
        Stack<Card> Cards { get; set; }
        DeckBase Shuffle();
    }
}
