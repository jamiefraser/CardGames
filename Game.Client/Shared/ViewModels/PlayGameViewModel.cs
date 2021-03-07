using Game.Client.Shared;
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
            Players = new SortedList<int, Player>();
            selectedcards = new ObservableCollection<Card>();
            //Players.CollectionChanged += PlayersChanged;
            rtc.PlayerDealtNewHand += PlayerDealtNewHand;
            rtc.TableStarted += TableStarted;
            rtc.PlayerRequestingToJoinTable += Rtc_PlayerRequestingToJoinTable;
            rtc.CardAddedToDiscardPile += CardAddedToDiscardPile;
            rtc.PlayerSelectedCard += PlayerSelectedCard;
            selectedcards.CollectionChanged += (s, a) =>
            {
                RaisePropertyChanged("CanDiscard");
            };
        }

        private void PlayerSelectedCard(object sender, PlayerSelectedCardEventArgs e)
        {
            Console.WriteLine($"A remote player selected a card: {e.Message.CardIndex}");
            var p = this.Players.Values.Where(player => player.PrincipalId.Equals(e.Message.Player.PrincipalId)).FirstOrDefault();
            p.Hand[e.Message.CardIndex].Selected = !p.Hand[e.Message.CardIndex].Selected;
            RaisePropertyChanged("DiscardPile");
        }

        private void CardAddedToDiscardPile(object sender, NewCardOnDiscardPileEventArgs e)
        {
            this.table.DiscardPile.Cards.Push(e.Card);
            RaisePropertyChanged("DiscardPile");
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
        public bool CannotDiscard
        {
            get
            {
                return selectedcards.Count != 1;
            }
        }
        private ObservableCollection<Entities.Card> selectedcards;
        public ObservableCollection<Entities.Card>SelectedCards
        {
            get
            {
                return selectedcards;
            }
            set
            {
                selectedcards = value;
                RaisePropertyChanged("SelectedCards");
            }
        }
        private bool roundcompleted;
        public bool RoundCompleted
        {
            get
            {
                return roundcompleted;
            }
            set
            {
                roundcompleted = value;
                RaisePropertyChanged("RoundCompleted");
            }
        }
        private Player dealer;
        public Player Dealer
        {
            get
            {
                return dealer;
            }
            set
            {
                dealer = value;
                RaisePropertyChanged("Dealer");
            }
        }
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
        private SortedList<int,Player> players;
        public SortedList<int,Player>Players
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
            foreach(KeyValuePair<int,Player> p in Table.Players.Where(player => !player.Value.PrincipalId.Equals(currentUserService.CurrentClaimsPrincipalOid)))
            {
                p.Value.Hand = new List<Card>();
                for(int i=0;i<Table.Game.NumberOfCardsToDeal;i++)
                {
                    p.Value.Hand.Add(new Card()
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
            Players = t.Players;
            PlayersRequestingEntry = new ObservableCollection<Player>(t.PlayersRequestingAccess);
            Player = Players.Where(p => p.Value.PrincipalId.Equals(currentUserService.CurrentClaimsPrincipalOid)).FirstOrDefault().Value;
            var service = factory.CreateClient("tableAPI");
            try
            {
                Started = table.Started;
                if(table.Dealer != null)
                {
                    Dealer = table.Dealer;
                }
                else
                {
                    Dealer = Players.Values.First();
                    table.Dealer = Players.Values.First();
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
                int key = Players.Count == 0 ? 0 : Players.Keys.Max()+1;
                Players.Add(key, PlayerToAdmit);
                //table.Players.Add(key,PlayerToAdmit);
                PlayersRequestingEntry.Remove(remove);
                PlayerToAdmit = null;
                if(Players.Count >= Table.Game.MinimumPlayers)
                {
                    CanStartGame = true;
                }
                RaisePropertyChanged("PlayersRequestingEntry");
                RaisePropertyChanged("Players");
            }
        }

        public async Task Decline(Entities.RequestToJoinTableMessage message)
        {
            throw new NotImplementedException();
        }

        public void PickupFromDiscardPile(Player player)
        {
            throw new NotImplementedException();
        }
        public void PickupFromDeck(Player player)
        {
            throw new NotImplementedException();
        }
        public void PlaySelectedCards(Player player, List<Card> cards)
        {
            throw new NotImplementedException();
        }
        public async Task DiscardSelectedCards()
        {
            var client = factory.CreateClient("tableAPI");
            await client.PostAsJsonAsync<Entities.Card>($"api/tables/hand/{this.table.Id.ToString()}/discard", this.selectedcards.First().ConvertSuitToColour());
            Player.Hand.Remove(this.selectedcards.First());
            this.selectedcards.Clear();
            foreach(Card c in player.Hand)
            {
                c.Selected = false;
            }
            RaisePropertyChanged("Hand");
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            rtc.PlayerRequestingToJoinTable -= Rtc_PlayerRequestingToJoinTable;
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
            await Deal();
        }


        #endregion
    }
}
