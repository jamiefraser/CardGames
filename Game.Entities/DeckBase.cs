using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class DeckBase :IDeck
    {
        [JsonProperty]
        public Guid Id { get; set; }
        public Queue<Card> Cards { get; set; }
        public DeckBase Shuffle()
        {
            Card[] cards = this.Cards.ToArray();
            this.Cards.Clear();
            for (int iteration = 0; iteration < 1000; iteration++)
            {
                var random = new Random();
                for (int t = 0; t < cards.Length; t++)
                {
                    Card tmp = cards[t];
                    int r = random.Next(t, cards.Length);
                    cards[t] = cards[r];
                    cards[r] = tmp;
                }
            }
            var c = new List<Card>(cards);
            foreach (Card card in c)
            {
                this.Cards.Enqueue(card);
            }
            return this;
        }
    }
}
