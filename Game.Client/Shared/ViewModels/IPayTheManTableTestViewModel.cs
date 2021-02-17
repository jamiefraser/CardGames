using Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.ViewModels
{
    public interface IPayTheManTableTestViewModel
    {
        List<Player> Players { get; set; }
        List<Card> Hand { get; set; }
        Entities.DeckType DeckType { get; set; }
    }
}
