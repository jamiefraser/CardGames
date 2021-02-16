using System;
using System.Drawing;

namespace Game.Entities
{
    public class Card
    {
        public string Suit
        {
            get; set;
        }
        public int Rank { get; set; }
        public bool IsSpecialCard = false;
        public bool Selected { get; set; }
        public Card()
        {
            Selected = false;
        }
        public Card(int rank, string suit)
        {
            Rank = rank;
            Suit = suit;
            IsSpecialCard = false;
            Selected = false;
        }
        public Card(int rank, string suit, bool isSpecialCard)
        {
            Rank = rank;
            Suit = suit;
            IsSpecialCard = isSpecialCard;
            Selected = false;
        }
    }
}
