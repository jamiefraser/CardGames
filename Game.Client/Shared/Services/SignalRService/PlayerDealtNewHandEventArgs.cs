using Game.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.Services.SignalRService
{
    public class PlayerDealtNewHandEventArgs : EventArgs
    {
        public ObservableCollection<Card> Hand { get; set; }
    }
}
