using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Game.Client.Shared.Services.SignalRService
{
    public interface ISignalRService
    {
        string AccessToken { get; set; }
        ObservableCollection<Entities.Player> PlayersOnline { get; set; }
        event EventHandler<PlayerAddedEventArgs> PlayerAdded;
        event EventHandler<PlayerRemovedEventArgs> PlayerRemoved;
    }
}
