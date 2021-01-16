using Game.Client.Shared.Services.CurrentUser;
using Game.Client.Shared.Services.SignalRService;
using Game.Client.Shared.ViewModels;
using Game.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Game.Client.Shared.ViewModels
{
    public class StartAGameViewModel : IStartAGameViewModel, IDisposable
    {
        #region Members
        private readonly ISignalRService signalRService;
        private readonly IHttpClientFactory factory;
        private readonly ICurrentUserService currentUserService;
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region ctor
        public StartAGameViewModel(ISignalRService _signalRService, IHttpClientFactory _factory, ICurrentUserService _currentUserService)
        {
            signalRService = _signalRService;
            factory = _factory;
            currentUserService = _currentUserService;
            GameTable = new Entities.Table();
            gametable.InvitedPlayers = new List<Player>();
            Players = new ObservableCollection<Entities.Player>();
            foreach (var p in signalRService.PlayersOnline.Where(player => !player.PrincipalId.Equals(currentUserService.CurrentClaimsPrincipalOid)))
            {
                Players.Add(p);
            }
            signalRService.PlayerAdded += SignalRService_PlayerAdded;
            signalRService.PlayerRemoved += SignalRService_PlayerRemoved;

            signalRService.TableAdded += SignalRService_TableAdded;
            signalRService.TableRemoved += SignalRService_TableRemoved;

            var client = factory.CreateClient("gameAPI");
            Task.Run(async () =>
            {
                Games = await client.GetFromJsonAsync<ObservableCollection<Game.Entities.Game>>("api/game");
                Console.WriteLine(Games.Count);
            });
            var tableClient = factory.CreateClient("tableAPI");
            Task.Run(async () =>
            {
                AvailableGameTables = await tableClient.GetFromJsonAsync<ObservableCollection<Entities.Table>>("api/tables");
                Console.WriteLine($"The are {availablegametables.Count()} tables available to join");
            });
        }

        private void SignalRService_TableRemoved(object sender, TableRemovedEventArgs e)
        {
            AvailableGameTables.Remove(e.Table);
            RaisePropertyChanged("AvailableGameTables");
        }

        private void SignalRService_TableAdded(object sender, TableAddedEventArgs e)
        {
            if (AvailableGameTables == null) availablegametables = new ObservableCollection<Table>();
            var t = AvailableGameTables.Where(table => table.Id.Equals(e.Table.Id)).FirstOrDefault();
            if (t == null)
            {
                AvailableGameTables.Add(e.Table);
            }
            else
            {
                AvailableGameTables.Remove(t);
                RaisePropertyChanged("AvailableGameTables");
                AvailableGameTables.Add(e.Table);
            }
            RaisePropertyChanged("AvailableGameTables");
        }
        #endregion

        #region Methods
        public async Task StartGame()
        {
            gametable.Game = selectedgame;

            var tableService = factory.CreateClient("tableAPI");
            try
            {
                var result = await tableService.PostAsJsonAsync<Entities.Table>("/api/tables", GameTable);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            signalRService.PlayerAdded -= SignalRService_PlayerAdded;
            signalRService.PlayerRemoved -= SignalRService_PlayerRemoved;
        }
        #endregion

        #region Event Handlers
        private void SignalRService_PlayerRemoved(object sender, PlayerRemovedEventArgs e)
        {
            var p = Players.Where(player => player.PrincipalId.Equals(e.Player.PrincipalId)).FirstOrDefault();
            if (p != null) Players.Remove(p);
            RaisePropertyChanged("Players");
        }

        private void SignalRService_PlayerAdded(object sender, PlayerAddedEventArgs e)
        {
            var p = Players.Where(player => player.PrincipalId.Equals(e.Player.PrincipalId)).FirstOrDefault();
            if (p == null)
            {
                Players.Add(e.Player);
            }
            RaisePropertyChanged("Players");
        }
        #endregion

        #region Properties
        private ObservableCollection<Entities.Table> availablegametables;
        public ObservableCollection<Entities.Table> AvailableGameTables
        {
            get
            {
                return availablegametables;
            }
            private set
            {
                availablegametables = value;
                RaisePropertyChanged("AvailableGameTables");
            }
        }
        private ObservableCollection<Entities.Player> players;
        public ObservableCollection<Entities.Player> Players
        {
            get
            {
                return players;
            }
            set
            {
                players = value;
            }
        }
        private ObservableCollection<Entities.Game> games;
        public ObservableCollection<Entities.Game> Games
        {
            get
            {
                return games;
            }
            set
            {
                games = value;
                RaisePropertyChanged("Games");
            }
        }
        private Entities.Game selectedgame;
        [Required(ErrorMessage = "You must select a game to play")]
        [ValidGameValidator(ErrorMessage = "You must select a game to play")]
        public Entities.Game SelectedGame
        {
            get
            {
                return selectedgame;
            }
            set
            {
                selectedgame = value;
                RaisePropertyChanged("SelectedGame");
            }
        }
        private Entities.Table gametable;
        public Entities.Table GameTable
        {
            get
            {
                return gametable;
            }
            set
            {
                gametable = value;
                RaisePropertyChanged("GameTable");
            }
        }
        public string[] InvitedPlayerIds
        { get; set; }
        #endregion
    }
}
