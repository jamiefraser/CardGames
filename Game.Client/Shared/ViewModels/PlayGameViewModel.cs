using Game.Client.Shared.Services.SignalRService;
using Game.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        #endregion

        #region ctors
        public PlayGameViewModel(ISignalRService _rtc,  IHttpClientFactory _factory)
        {
            rtc = _rtc;
            factory = _factory;
            rtc.PlayerRequestingToJoinTable += Rtc_PlayerRequestingToJoinTable;
            PlayersRequestingEntry = new ObservableCollection<Player>();
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
        #endregion

        #region Methods
        public async Task Initialize(string tableId)
        {
            if (rtc.AvailableTables.Equals(null) || rtc.AvailableTables.Count == 0) await rtc.InitializeAsync();
            var t = rtc.AvailableTables.Where(table => table.Id.Equals(Guid.Parse(tableId))).FirstOrDefault();
            Table = t;
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
        }


        #endregion
    }
}
