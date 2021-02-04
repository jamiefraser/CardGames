using Game.Client.Shared.Services.CurrentUser;
using Game.Client.Shared.Services.SignalRService;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Client.Shared.ViewModels
{
    public class LobbyViewModel : ViewModelBase,  ILobbyViewModel, IDisposable
    {
        #region Members
        private readonly ISignalRService signalRService;
        private readonly IHttpClientFactory factory;
        private readonly ICurrentUserService currentUserService;
        private readonly NavigationManager nav;
        #endregion

        #region ctor
        public LobbyViewModel(ISignalRService _signalRService, IHttpClientFactory _factory, ICurrentUserService _currentUserService,  NavigationManager _nav)
        {
            signalRService = _signalRService;
            factory = _factory;
            currentUserService = _currentUserService;
            nav = _nav;
            signalRService.PlayerAdmittedToTable += SignalRService_PlayerAdmittedToTable;
        }

        private void SignalRService_PlayerAdmittedToTable(object sender, PlayerRequestingToJoinTableEventArgs e)
        {
            WaitingForPermissionToJoin = false;
            if (e.Message.RequestingPlayer.PrincipalId.Equals(currentUserService.CurrentClaimsPrincipalOid))
            {
                nav.NavigateTo($"/games/play/{Table.Id}");
            }
        }
        #endregion

        #region Properties
        private bool waitingforpermissiontojoin = true;
        public bool WaitingForPermissionToJoin
        {
            get
            {
                return waitingforpermissiontojoin;
            }
            set
            {
                waitingforpermissiontojoin = value;
                RaisePropertyChanged("WaitingForPermissionToJoin");
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
        #endregion

        #region Methods
        public async void Initialize(Guid tableId)
        {
            try
            {
                if (signalRService.AvailableTables == null || signalRService.AvailableTables.Count == 0)
                {
                    await signalRService.InitializeAsync();
                }
 
                Table = signalRService.AvailableTables.Where(t => t.Id.Equals(tableId)).FirstOrDefault();
                if (table.InvitedPlayers == null) table.InvitedPlayers = new List<Entities.Player>();
                if (table.InvitedPlayers.Union(table.Players).Where(p => p.PrincipalId.Equals(currentUserService.CurrentClaimsPrincipalOid)).Count() > 0)
                {
                    WaitingForPermissionToJoin = false;
                    nav.NavigateTo($"/games/play/{Table.Id}");
                }
                else
                {
                    //Send request to join message
                    var req = new Entities.RequestToJoinTableMessage()
                    {
                        RequestingPlayer = currentUserService.CurrentClaimsPrincipal.ToPlayer(),
                        Table = table,
                        TableOwnerId = table.TableOwner.PrincipalId
                    };
                    var tableClient = factory.CreateClient("tableAPI");
                    await tableClient.PostJsonAsync("/api/tables/join", table);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}\r\n{ex.StackTrace}");
            }
        }


        #endregion

        #region IDisposable
        public void Dispose()
        {
            signalRService.PlayerAdmittedToTable -= SignalRService_PlayerAdmittedToTable;
        }
        #endregion
    }
}
