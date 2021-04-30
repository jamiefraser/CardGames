using Game.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Game.Client.Shared.Services.SignalRService
{
    public interface ISignalRService
    {
        event EventHandler<ReadyStateChangedEventArgs> ReadyStateChanged;
        string AccessToken { get; set; }
        ObservableCollection<Entities.Player> PlayersOnline { get; set; }
        ObservableCollection<Entities.Table> AvailableTables { get; }
        event EventHandler<PlayerAddedEventArgs> PlayerAdded;
        event EventHandler<PlayerRemovedEventArgs> PlayerRemoved;
        event EventHandler<TableAddedEventArgs> TableAdded;
        event EventHandler<TableRemovedEventArgs> TableRemoved;
        event EventHandler<PlayerRequestingToJoinTableEventArgs> PlayerAdmittedToTable;
        event EventHandler<PlayerRequestingToJoinTableEventArgs> PlayerRequestingToJoinTable;
        event EventHandler<PlayerDealtNewHandEventArgs> PlayerDealtNewHand;
        event EventHandler<TableStartedEventArgs> TableStarted;
        event EventHandler<NewCardOnDiscardPileEventArgs> CardAddedToDiscardPile;
        event EventHandler<PlayerSelectedCardEventArgs> PlayerSelectedCard;
        Task InitializeAsync();
        Task DisconnectSignalR();
        Task SayHello(string message);
        Task Deal(string tableId);
        Task UpdateStatus(bool online);
        Task CreateTable(Table table);
        Task RequestToJoinTable(RequestToJoinTableMessage message);
    }
}
