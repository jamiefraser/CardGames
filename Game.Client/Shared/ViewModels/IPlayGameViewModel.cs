using Game.Entities;
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
        SortedList<int, Entities.Player> Players { get; set; }
        Task Admit();
        Task Decline(Entities.RequestToJoinTableMessage message);
        Entities.Table Table { get; set; }
        Entities.Player PlayerToAdmit { get; set; }
        Task Initialize(string tableId);
        Task Deal();
        Task StartGame();
        bool CanStartGame { get; set; }
        bool Started { get; set; }
        Player Player { get; set; }
        Player Dealer { get; set; }
        bool RoundCompleted { get; set; }
        void PickupFromDiscardPile(Player player);
        void PickupFromDeck(Player player);
        void PlaySelectedCards(Player player, List<Card> cards);
        Task DiscardSelectedCards();
        ObservableCollection<Entities.Card> SelectedCards { get; set; }
        bool CannotDiscard { get; }
    }
}
