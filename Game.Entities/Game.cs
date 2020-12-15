using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class Game : Microsoft.Azure.Cosmos.Table.TableEntity
    {
        public int NumberOfDecks { get; set; }
        public DeckType DeckType { get; set; }
        public int MinimumPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public bool UsesDiscardPile { get; set; }
        public bool PlayerPicksUpWholeDiscardPile { get; set; }
        public int NumberOfCardsToDeal { get; set; }

        public bool ProgressiveDeal { get; set; }
        public int IncrementCardsToDealBy { get; set; }
    }
}
