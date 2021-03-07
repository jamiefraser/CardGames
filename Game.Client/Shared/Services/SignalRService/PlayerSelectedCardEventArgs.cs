using Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.Services.SignalRService
{
    public class PlayerSelectedCardEventArgs
    {
        public PlayerSelectedCardEventArgs(CardSelectedMessage message)
        {
            Message = message;
        }
        public CardSelectedMessage Message { get; set; }
    }
}
