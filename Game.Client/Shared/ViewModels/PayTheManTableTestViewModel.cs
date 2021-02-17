using Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.ViewModels
{
    public class PayTheManTableTestViewModel : IPayTheManTableTestViewModel
    {
        public PayTheManTableTestViewModel()
        {
            Hand = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Card>>(handjson);
            DeckType = DeckType.Phase10;
        }
        private string handjson = "[{'IsSpecialCard':false,'Suit':'red','Rank':1},{'IsSpecialCard':false,'Suit':'red','Rank':11},{'IsSpecialCard':false,'Suit':'green','Rank':4},{'IsSpecialCard':false,'Suit':'blue','Rank':4},{'IsSpecialCard':false,'Suit':'red','Rank':5},{'IsSpecialCard':false,'Suit':'red','Rank':10},{'IsSpecialCard':false,'Suit':'red','Rank':7},{'IsSpecialCard':false,'Suit':'green','Rank':4},{'IsSpecialCard':false,'Suit':'red','Rank':9},{'IsSpecialCard':false,'Suit':'wild','Rank':100},{'IsSpecialCard':false,'Suit':'skip','Rank':200},{'IsSpecialCard':false,'Suit':'blue','Rank':8},{'IsSpecialCard':false,'Suit':'yellow','Rank':3}]";
        public List<Player> Players { get; set; }
        public List<Card> Hand { get; set; }
        public Entities.DeckType DeckType { get; set; }
    }
}
