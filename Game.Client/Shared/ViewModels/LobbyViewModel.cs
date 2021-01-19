using Game.Client.Shared.Services.CurrentUser;
using Game.Client.Shared.Services.SignalRService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.ViewModels
{
    public class LobbyViewModel : ViewModelBase,  ILobbyViewModel
    {
        #region Members
        private readonly ISignalRService signalRService;
        private readonly IHttpClientFactory factory;
        private readonly ICurrentUserService currentUserService;
        #endregion

        #region ctor
        public LobbyViewModel(ISignalRService _signalRService, IHttpClientFactory _factory, ICurrentUserService _currentUserService)
        {
            signalRService = _signalRService;
            factory = _factory;
            currentUserService = _currentUserService;
            
        }
        #endregion

        #region Properties
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
            if(signalRService.AvailableTables==null || signalRService.AvailableTables.Count==0)
            {
                await signalRService.Initialize();
            }
            Table = signalRService.AvailableTables.Where(t => t.Id.Equals(tableId)).FirstOrDefault();
            
        }
        #endregion
    }
}
