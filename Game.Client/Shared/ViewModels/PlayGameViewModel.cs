using Game.Client.Shared.Services.CurrentUser;
using Game.Client.Shared.Services.SignalRService;
using Game.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;

using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.ViewModels
{
    public class PlayGameViewModel : ViewModelBase,  IPlayGameViewModel, IDisposable
    {
        #region members
        private ISignalRService rtc;
        private readonly IHttpClientFactory factory;
        private readonly ICurrentUserService currentUserService;
        #endregion

        #region ctors
        public PlayGameViewModel(ISignalRService _rtc,  IHttpClientFactory _factory, ICurrentUserService _currentUserService)
        {
            rtc = _rtc;
            currentUserService = _currentUserService;
            factory = _factory;
            PlayersRequestingEntry = new ObservableCollection<Player>();
            Players = new ObservableCollection<Player>();
            Players.CollectionChanged += PlayersChanged;
            rtc.PlayerDealtNewHand += PlayerDealtNewHand;
            rtc.TableStarted += TableStarted;
            rtc.PlayerRequestingToJoinTable += Rtc_PlayerRequestingToJoinTable;
        }

        private void TableStarted(object sender, TableStartedEventArgs e)
        {
            Console.WriteLine($"Received a table started message for {e.TableId}");
            Started = true;
            Table.Started = true;
            RaisePropertyChanged("Table");
        }

        private void PlayerDealtNewHand(object sender, PlayerDealtNewHandEventArgs e)
        {
            Player.Hand = e.Hand.ToList();
            RaisePropertyChanged("Player");
        }

        private void PlayersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Players");
        }

        private void Rtc_PlayerRequestingToJoinTable(object sender, PlayerRequestingToJoinTableEventArgs e)
        {
            if (playersrequestingentry == null) playersrequestingentry = new ObservableCollection<Entities.Player>();
            PlayersRequestingEntry.Add(e.Message.RequestingPlayer);
            Table.PlayersRequestingAccess.Add(e.Message.RequestingPlayer);
            RaisePropertyChanged("PlayersRequestingEntry");
        }
        #endregion

        #region Properties
        private Player player;
        public Player Player
        {
            get
            {
                return player;
            }
            set
            {
                player = value;
                RaisePropertyChanged("Player"); 
            }
        }
        private bool started = false;
        public bool Started
        {
            get
            {
                return started;
            }
            set
            {
                started = value;
                if (started)
                {
                    CanStartGame = false;
                }
                RaisePropertyChanged("Started");
            }
        }
        private bool canstartgame = false;
        public bool CanStartGame
        {
            get
            {
                return canstartgame;
            }
            set
            {
                canstartgame = value;
                RaisePropertyChanged("CanStartGame");
            }
        }
        private ObservableCollection<Entities.Player> playersrequestingentry;
        public ObservableCollection<Entities.Player>PlayersRequestingEntry
        {
            get
            {
                return playersrequestingentry;
            }
            set
            {
                playersrequestingentry = value;
                RaisePropertyChanged("PlayersRequestingEntry");
            }
        }
        private Entities.Table table;
        public Entities.Table Table
        {
            get
            {
                return table;
            }
            set
            {
                table = value;
                RaisePropertyChanged("Table");
            }
        }

        private Entities.Player playertoadmit;
        public Entities.Player PlayerToAdmit
        {
            get
            {
                return playertoadmit;
            }
            set
            {
                playertoadmit = value;
                RaisePropertyChanged("PlayerToAdmit");
            }
        }
        private ObservableCollection<Player> players;
        public ObservableCollection<Player>Players
        {
            get 
            {
                return players;
            }
            set
            {
                players = value;
                RaisePropertyChanged("Players");
            }
        }
        #endregion

        #region Methods
        public async Task Deal()
        {
            Console.WriteLine($"There are {Table.Players.Count} players at the table");
            var service = factory.CreateClient("tableAPI");
            var x = await service.PostAsJsonAsync($"api/tables/deal/{this.table.Id}","");
            var s = await x.Content.ReadAsStringAsync();
            foreach(Player p in Table.Players.Where(player => !player.PrincipalId.Equals(currentUserService.CurrentClaimsPrincipalOid)))
            {
                p.Hand = new List<Card>();
                for(int i=0;i<Table.Game.NumberOfCardsToDeal;i++)
                {
                    p.Hand.Add(new Card()
                    {
                        Suit = "red",
                        Rank = "0"
                    });
                }
            }
            RaisePropertyChanged("Table");
         }
        public async Task Initialize(string tableId)
        {
            if (rtc.AvailableTables.Equals(null) || rtc.AvailableTables.Count == 0) await rtc.InitializeAsync();
            var t = rtc.AvailableTables.Where(table => table.Id.Equals(Guid.Parse(tableId))).FirstOrDefault();
            Table = t;
            Players = new ObservableCollection<Player>(t.Players.ToArray());
            PlayersRequestingEntry = new ObservableCollection<Player>(t.PlayersRequestingAccess);
            Player = Players.Where(p => p.PrincipalId.Equals(currentUserService.CurrentClaimsPrincipalOid)).FirstOrDefault();
            var service = factory.CreateClient("tableAPI");
            try
            {
                var hand = await service.GetFromJsonAsync<List<Card>>($"api/tables/hand/{this.table.Id}");
                Player.Hand = hand;
                if(Player.Hand != null)
                {
                    Started = true;
                }
            }
            catch { }
        }
        public async Task Admit()
        {
            if (PlayerToAdmit == null || table == null) return;
            var message = new RequestToJoinTableMessage()
            {
                RequestingPlayer = PlayerToAdmit,
                Table = table,
                TableOwnerId = table.TableOwner.PrincipalId
            };
            var client = factory.CreateClient("tableAPI");
            var response = await client.PostAsJsonAsync("/api/tables/admit", message);
            if(response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Successfully admitted {playertoadmit.PrincipalName}");
                var remove = table.PlayersRequestingAccess.Where(p => p.PrincipalId.Equals(playertoadmit.PrincipalId)).FirstOrDefault();
                table.PlayersRequestingAccess.Remove(remove);
                Players.Clear();
                foreach(Player p in table.Players)
                {
                    Players.Add(p);
                }
                Players.Add(PlayerToAdmit);
                PlayersRequestingEntry.Remove(remove);
                PlayerToAdmit = null;
                if(Players.Count >= Table.Game.MinimumPlayers)
                {
                    CanStartGame = true;
                }
                RaisePropertyChanged("PlayersRequestingEntry");
            }
        }

        public async Task Decline(Entities.RequestToJoinTableMessage message)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            rtc.PlayerRequestingToJoinTable -= Rtc_PlayerRequestingToJoinTable;
            Players.CollectionChanged -= PlayersChanged;
            rtc.PlayerDealtNewHand -= PlayerDealtNewHand;
            rtc.TableStarted -= TableStarted;
        }

        public async Task StartGame()
        {
            //Started = true;
            //Table.Started = true;
            //rtc.AvailableTables.Where(tbl => tbl.Id.Equals(Table.Id)).FirstOrDefault()!.Started = true;
            var tableService = factory.CreateClient("tableAPI");
            var result = await tableService.PostAsJsonAsync<string>($"/api/tables/{Table.Id.ToString()}/start","");
        }


        #endregion
    }
}
