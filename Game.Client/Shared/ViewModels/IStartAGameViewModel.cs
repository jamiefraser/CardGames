using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Game.Client.Shared.ViewModels
{
    public interface IStartAGameViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Entities.Player> Players { get; set; }
        ObservableCollection<Entities.Game> Games { get; }
        ObservableCollection<Entities.Table> AvailableGameTables { get; }
        Entities.Table GameTable { get; set; }
        Entities.Game SelectedGame { get; set; }
        string[] InvitedPlayerIds { get; set; }
        Task StartGame();
    }
}
