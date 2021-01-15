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
        ObservableCollection<Entities.Table> AvailableTables { get; }
        event EventHandler<PlayerAddedEventArgs> PlayerAdded;
        event EventHandler<PlayerRemovedEventArgs> PlayerRemoved;
        event EventHandler<TableAddedEventArgs> TableAdded;
        event EventHandler<TableRemovedEventArgs> TableRemoved;
    }
}
