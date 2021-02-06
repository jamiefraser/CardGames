using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.ViewModels
{
    public interface IPlayGameViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Entities.Player> PlayersRequestingEntry { get; set; }
        ObservableCollection<Entities.Player> Players { get; set; }
        Task Admit();
        Task Decline(Entities.RequestToJoinTableMessage message);
        Entities.Table Table { get; set; }
        Entities.Player PlayerToAdmit { get; set; }
        Task Initialize(string tableId);
        Task Deal();
        Task StartGame();
        bool CanStartGame { get; set; }
        bool Started { get; set; }
    }
}
