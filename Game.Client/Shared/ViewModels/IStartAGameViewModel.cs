using Game.Client.Shared.Services.SignalRService;
using Game.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Game.Client.Shared.ViewModels
{
    public interface IStartAGameViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Entities.Player> Players { get; set; }
        ObservableCollection<Entities.Game> Games { get; }
        ObservableCollection<Entities.Table> AvailableGameTables { get; }
        bool CannotCreateGameTable { get;  }
        Entities.Table GameTable { get; set; }
        Entities.Game SelectedGame { get; set; }
        Entities.Table SelectedGameTable { get; set; }
        string[] InvitedPlayerIds { get; set; }
        Task StartGame();
        Task Initialize();
        event EventHandler<PlayerRequestingToJoinTableEventArgs> OwnGameCreated;
        Task sayHello(string message);
    }
}
