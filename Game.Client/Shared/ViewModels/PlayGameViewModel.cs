﻿using Game.Client.Shared.Services.CurrentUser;
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
            rtc.PlayerRequestingToJoinTable += Rtc_PlayerRequestingToJoinTable;
            PlayersRequestingEntry = new ObservableCollection<Player>();
            Players = new ObservableCollection<Player>();
            Players.CollectionChanged += PlayersChanged;
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
            var service = factory.CreateClient("tableAPI");
            var x = await service.PostAsJsonAsync($"api/tables/deal/{this.table.Id}","");
            var s = await x.Content.ReadAsStringAsync();
            var t = Newtonsoft.Json.JsonConvert.DeserializeObject<Entities.Table>(s);
            var h = t.Players.Where(p => p.PrincipalId.Equals(currentUserService.CurrentClaimsPrincipalOid)).FirstOrDefault().Hand;
            this.Table.Players.Where(p => p.PrincipalId.Equals(currentUserService.CurrentClaimsPrincipalOid)).FirstOrDefault().Hand = h;
            RaisePropertyChanged("Table");
         }
        public async Task Initialize(string tableId)
        {
            if (rtc.AvailableTables.Equals(null) || rtc.AvailableTables.Count == 0) await rtc.InitializeAsync();
            var t = rtc.AvailableTables.Where(table => table.Id.Equals(Guid.Parse(tableId))).FirstOrDefault();
            Table = t;
            Players = new ObservableCollection<Player>(t.Players.ToArray());
            PlayersRequestingEntry = new ObservableCollection<Player>(t.PlayersRequestingAccess);
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
        }


        #endregion
    }
}
