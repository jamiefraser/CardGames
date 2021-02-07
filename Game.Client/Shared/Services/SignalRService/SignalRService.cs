using Game.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Game.Client.Shared.Services.CurrentUser;

namespace Game.Client.Shared.Services.SignalRService
{
    public class SignalRService : ISignalRService
    {
        #region members
        private HubConnection hubConnection;
        private List<string> messages = new List<string>();
        private IHttpClientFactory _factory;
        private readonly ICurrentUserService currentUserService;
        private List<string> IDsOfTablesRequestedToJoin;
        #endregion

        #region ctors
        public SignalRService(IHttpClientFactory factory, string serviceBaseUrl, ICurrentUserService _currentUserService)
        {
            _factory = factory;
            IDsOfTablesRequestedToJoin = new List<string>();
            var clientAddress = factory.CreateClient("PresenceServiceRoot").BaseAddress;
            hubConnection = new HubConnectionBuilder()
                                .WithUrl($"{serviceBaseUrl}api")
                                .WithAutomaticReconnect()
                                .Build();
            currentUserService = _currentUserService;
            var client = factory.CreateClient("presenceAPI");
            var tableClient = factory.CreateClient("tableAPI");
            PlayersOnline = new ObservableCollection<Player>();
        }

        private void HandlePlayerRequestingToJoinTableMessage(RequestToJoinTableMessage message)
        {
            if (message.TableOwnerId.Equals(currentUserService.CurrentClaimsPrincipalOid))
            {
                RaisePlayerRequestingToJoinTable(message);
            }
            else
            {
                Console.WriteLine("Received a request to join a table, but it doesn't belong to the logged in user");
            }
        }

        private void HandlePlayerJoinedOrLeftMessage(ICurrentUserService _currentUserService, PresenceStatusMessage message)
        {
            Console.WriteLine($"{message.Player.PrincipalName} {(message.CurrentStatus.Equals(PlayerPresence.Online) ? " is online" : " is offline")}");
            if (message.CurrentStatus.Equals(PlayerPresence.Online))
            {
                if (PlayersOnline.Contains(message.Player)) return;
                if (message.Player.PrincipalId.Equals(_currentUserService.CurrentClaimsPrincipalOid)) return;
                var playerQry = from Entities.Player player in PlayersOnline
                                where player.PrincipalId.Equals(message.Player.PrincipalId)
                                select player;
                if (playerQry.Count() > 0) return;
                var p = new Player()
                {
                    ETag = message.Player.ETag,
                    Hand = message.Player.Hand,
                    PartitionKey = message.Player.PartitionKey,
                    PrincipalId = message.Player.PrincipalId,
                    PrincipalIdp = message.Player.PrincipalIdp,
                    PrincipalName = message.Player.PrincipalName,
                    RowKey = message.Player.RowKey,
                    Timestamp = message.Player.Timestamp
                };

                PlayersOnline.Add(p);
                RaisePlayerAdded(p);
            }
            else
            {
                var p = PlayersOnline.Where(po => po.PrincipalId.Equals(message.Player.PrincipalId)).FirstOrDefault();
                if (p != null) PlayersOnline.Remove(p);
                RaisePlayerRemoved(p);
            }
        }

        private void HandleTableCreatedOrDeleted(TableCreationOrDeletionMessage message)
        {
            if (message.Action.Equals(TableAction.Added))
            {
                Console.WriteLine($"New table created.  It belongs to user {currentUserService.CurrentClaimsPrincipal.ToPlayer().PrincipalName}.  There are now  {AvailableTables.Count()} tables to play");
                #region Check to see if the table already exists in which case this is *really* an update
                var t = AvailableTables.Where(tbl => tbl.Id.Equals(message.Table.Id)).FirstOrDefault();
                if (t != null)
                {
                    AvailableTables.Remove(t);
                }
                AvailableTables.Add(message.Table);
                RaiseTableAdded(message.Table);
                #endregion
            }
            else
            {
                var t = AvailableTables.Where(tbl => tbl.Id.Equals(message.Table.Id)).FirstOrDefault();
                AvailableTables.Remove(t);
                RaiseTableRemoved(message.Table);
            }
        }

        public async Task InitializeAsync()
        {
            await hubConnection.StartAsync();
            #region HubConnection Handlers
            hubConnection.On("newtable", (Action<TableCreationOrDeletionMessage>)(message =>
            {
                HandleTableCreatedOrDeleted(message);
            }));
            hubConnection.On("presence", (Action<PresenceStatusMessage>)((message) =>
            {
                HandlePlayerJoinedOrLeftMessage(currentUserService, message);
            }));
            hubConnection.On("joinrequest", (Action<RequestToJoinTableMessage>)((message) =>
            {
                HandlePlayerRequestingToJoinTableMessage(message);
            }));
            hubConnection.On<RequestToJoinTableMessage>("playeradmitted", (message) =>
            {
                var t = AvailableTables.Where(tbl => tbl.Id.Equals(message.Table.Id)).FirstOrDefault();
                if (t != null)
                {
                    availabletables.Remove(t);
                    availabletables.Add(message.Table);
                }
                RaisePlayerAdmittedToTable(message);
            });

            #endregion
            var clientAddress = _factory.CreateClient("PresenceServiceRoot").BaseAddress;
            var client = _factory.CreateClient("presenceAPI");
            var tableClient = _factory.CreateClient("tableAPI");
            var players = await client.GetFromJsonAsync<List<Entities.Player>>("api/players");
            PlayersOnline = new ObservableCollection<Entities.Player>(players);
            try
            {
                var tables = await tableClient.GetFromJsonAsync<List<Entities.Table>>("api/tables");
                AvailableTables = new ObservableCollection<Table>(tables);
                RaiseReadyStateChanged(true);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
                Console.WriteLine($"The tableClient {(tableClient == null ? "is" : "is not")} null");
                availabletables = new ObservableCollection<Table>();
                RaiseReadyStateChanged(false);
            }
        }
        public async Task DisconnectSignalR()
        {
            await hubConnection.StopAsync();
            #region HubConnection Handlers
            hubConnection.Remove("newtable");
            hubConnection.Remove("presence");
            hubConnection.Remove("joinrequest");
            hubConnection.Remove("playeradmitted");

            #endregion
        }
        #endregion

        #region Properties
        private ObservableCollection<Entities.Player> _playersonline;

        public event EventHandler<PlayerAddedEventArgs> PlayerAdded;
        public event EventHandler<PlayerRemovedEventArgs> PlayerRemoved;
        public event EventHandler<PlayerRequestingToJoinTableEventArgs> PlayerRequestingToJoinTable;
        private void RaisePlayerRequestingToJoinTable(Entities.RequestToJoinTableMessage message)
        {
            if(PlayerRequestingToJoinTable != null)
            {
                PlayerRequestingToJoinTable(this, new PlayerRequestingToJoinTableEventArgs()
                {
                    Message = message
                });
            }
        }
        private void RaisePlayerAdded(Entities.Player player)
        {
            if(PlayerAdded != null)
            {
                PlayerAdded(this,new PlayerAddedEventArgs()
                {
                    Player = player
                });
            }
        }
        private void RaisePlayerRemoved(Entities.Player player)
        {
            if(PlayerRemoved != null)
            {
                PlayerRemoved(this, new PlayerRemovedEventArgs()
                {
                    Player = player
                });
            }
        }
        public event EventHandler<ReadyStateChangedEventArgs> ReadyStateChanged;
        public event EventHandler<PlayerRequestingToJoinTableEventArgs> PlayerAdmittedToTable;
        public event EventHandler<TableAddedEventArgs> TableAdded;
        public event EventHandler<TableRemovedEventArgs> TableRemoved;

        private void RaiseReadyStateChanged(bool Ready)
        {
            if(ReadyStateChanged!=null)
            {
                {
                    ReadyStateChanged(this, new ReadyStateChangedEventArgs() { Ready = Ready });
                } }
        }
        private void RaiseTableAdded(Entities.Table table)
        {
            if(TableAdded!=null)
            {
                TableAdded(this, new TableAddedEventArgs()
                {
                    Table = table
                });
            }
        }

        private void RaiseTableRemoved(Entities.Table table)
        {
            if(TableRemoved != null)
            {
                TableRemoved(this, new TableRemovedEventArgs()
                {
                    Table = table
                });
            }
        }

        private void RaisePlayerAdmittedToTable(RequestToJoinTableMessage message)
        {
            if(PlayerAdmittedToTable != null)
            {
                PlayerAdmittedToTable(this, new PlayerRequestingToJoinTableEventArgs() 
                { 
                    Message = message 
                });
            }
        }

        private ObservableCollection<Entities.Table> availabletables;
        public ObservableCollection<Entities.Table>AvailableTables
        {
            get
            {
                return availabletables;
            }
            private set
            {
                availabletables = value;
            }
        }

        public ObservableCollection<Entities.Player>PlayersOnline
        {
            get
            {
                return _playersonline;
            }
            set
            {
                _playersonline = value;
            }
        }

        public string AccessToken
        {
            get;
            set;
        }
        #endregion
    }
}
