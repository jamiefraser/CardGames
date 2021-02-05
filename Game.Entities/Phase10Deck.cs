using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Entities
{
    public class Phase10Deck : DeckBase, IDeck
    {

        public Phase10Deck()
        {
            List<Card> cards = new List<Card>();
            for (int i = 0; i < 2; i++)
            {
                for (int suit = 0; suit < standardsuits.Count(); suit++)
                {
                    for (int card = 0; card < standardcards.Count(); card++)
                    {
                        cards.Add(new Card(card, standardsuits[suit]));
                    }
                }
            }
            for(int i = 0;i<8;i++)
            {
                cards.Add(new Card(100, "Wild", true));
            }
            for(int i=0;i<4;i++)
            {
                cards.Add(new Card(200, "Skip", true));
            }
            this.Cards = new Queue<Card>();
            this.Cards.Clear();
            foreach (Card c in cards)
            {
                this.Cards.Enqueue(c);
            }
            Id = Guid.NewGuid();
        }
        static string[] standardcards = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
        static string[] standardsuits = new string[] { "red", "blue", "yellow", "green" };
    }
}
