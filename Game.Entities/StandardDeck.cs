﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Entities
{
    public class StandardDeck : DeckBase, IDeck
    {
        public StandardDeck(bool includeWilds = false)
        {
            List<Card> cards = new List<Card>();
            for (int suit = 0; suit < standardsuits.Count(); suit++)
            {
                for (int card = 0; card < standardcards.Count(); card++)
                {
                    cards.Add(new Card(standardcards[card], standardsuits[suit]));
                }
            }
            if (includeWilds)
            {
                foreach (string s in wildcards)
                {
                    cards.Add(new Card(100.ToString(), s, true));
                }
            }
            this.Cards = new Stack<Card>();
            this.Cards.Clear();
            foreach (Card c in cards)
            {
                this.Cards.Push(c);
            }
            Id = Guid.NewGuid();
        }
        static string[] standardcards = new string[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        static string[] standardsuits = new string[] { "diamonds", "hearts", "spades", "clubs" };
        static string[] wildcards = new string[] { "Red Joker", "Black Joker" };
    }
}
