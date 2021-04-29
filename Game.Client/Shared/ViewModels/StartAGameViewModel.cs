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
using Game.Client.Shared;
using Microsoft.AspNetCore.Components;
using System.Windows.Input;
using Microsoft.AspNet.SignalR.Messaging;

namespace Game.Client.Shared.ViewModels
{
    public class StartAGameViewModel : ViewModelBase,  IStartAGameViewModel, IDisposable
    {
        #region Members
        private readonly ISignalRService signalRService;
        private readonly IHttpClientFactory factory;
        private readonly ICurrentUserService currentUserService;
        private readonly NavigationManager nav;
        #endregion

        #region ctor
        public StartAGameViewModel(ISignalRService _signalRService, IHttpClientFactory _factory, ICurrentUserService _currentUserService, NavigationManager _nav)
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
        }
       public async Task sayHello(string message)
        {
            signalRService.SayHello(message);
        }
        public async Task Initialize()
        {
            var tableClient = factory.CreateClient("tableAPI");
            if (signalRService.AvailableTables == null) await signalRService.InitializeAsync();
            AvailableGameTables = new ObservableCollection<Table>(signalRService.AvailableTables);
            Console.WriteLine($"The are {availablegametables.Count()} tables available to join");
        }
        private void SignalRService_TableRemoved(object sender, TableRemovedEventArgs e)
        {
            AvailableGameTables.Remove(e.Table);
            RaisePropertyChanged("AvailableGameTables");
        }

        private void SignalRService_TableAdded(object sender, TableAddedEventArgs e)
        {
            if (AvailableGameTables == null) availablegametables = signalRService.AvailableTables;
            var t = AvailableGameTables.Where(table => table.Id.Equals(e.Table.Id)).FirstOrDefault();
            if (t == null)
            {
                AvailableGameTables.Add(e.Table);
                RaisePropertyChanged("AvailableGameTables");
            }
            else
            {
                AvailableGameTables.Remove(t);
                RaisePropertyChanged("AvailableGameTables");
                AvailableGameTables.Add(e.Table);
                RaisePropertyChanged("AvailableGameTables");

            }
            if (e.Table.TableOwner.PrincipalId.Equals(currentUserService.CurrentClaimsPrincipalOid))
            {
                RaiseOwnGameCreated(new RequestToJoinTableMessage()
                {
                    Table = e.Table,
                    TableOwnerId = e.Table.TableOwner.PrincipalId
                });
            }
            RaisePropertyChanged("AvailableGameTables");
        }
        #endregion

        #region Methods
        public async Task StartGame()
        {
            gametable.Game = selectedgame;

            //var tableService = factory.CreateClient("tableAPI");
            //gametable.InvitedPlayers.Add(currentUserService.CurrentClaimsPrincipal.ToPlayer());
            gametable.Players.Add(0, currentUserService.CurrentClaimsPrincipal.ToPlayer());
            gametable.Dealer = currentUserService.CurrentClaimsPrincipal.ToPlayer();

            List<string> ids = gametable.InvitedPlayerIds != null ? new List<string>(gametable.InvitedPlayerIds) : new List<string>();
            ids.Add(currentUserService.CurrentClaimsPrincipalOid);
            gametable.InvitedPlayerIds = ids.ToArray();
            await signalRService.CreateTable(gametable);
            //try
            //{
            //    var result = await tableService.PostAsJsonAsync<Entities.Table>("/api/tables", GameTable);
            //}
            //catch (Exception ex)
            //{

            //}
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            signalRService.PlayerAdded -= SignalRService_PlayerAdded;
            signalRService.PlayerRemoved -= SignalRService_PlayerRemoved;
        }
        #endregion

        #region Events
        public event EventHandler<PlayerRequestingToJoinTableEventArgs> OwnGameCreated;

        private void RaiseOwnGameCreated(RequestToJoinTableMessage message)
        {
            if(OwnGameCreated != null)
            {
                OwnGameCreated(this, new PlayerRequestingToJoinTableEventArgs()
                {
                    Message = message
                });
            }
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
        private Entities.Table selectedgametable;
        public Entities.Table SelectedGameTable
        {
            get
            {
                return selectedgametable;
            }
            set
            {
                selectedgametable = value;
                RaisePropertyChanged("SelectedGameTable");
            }
        }
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
        public bool CannotCreateGameTable
        {
            get
            {
                return SelectedGame == null || string.IsNullOrEmpty(gametable.Name);
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
