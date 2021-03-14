using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.Services.SignalRService
{
    public class TableStartedEventArgs
    {
        public string TableId { get; set; }
        public Entities.Player Dealer { get; set; }
    }
}
