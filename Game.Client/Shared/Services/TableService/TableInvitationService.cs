using Game.Client.Shared.Services.SignalRService;
using Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
namespace Game.Client.Shared.Services.TableInvitationService
{
    public class TableInvitationService : ITableInvitationService
    {
        #region Members
        private readonly ISignalRService signalRService;
        private readonly IHttpClientFactory httpClientFactory;
        #endregion

        #region ctors
        public TableInvitationService(ISignalRService _signalRService, IHttpClientFactory _httpClientFactory)
        {
            signalRService = _signalRService;
            httpClientFactory = _httpClientFactory;
            
        }
        #endregion

        #region Methods
        public void CancelRequestForInvitation(Table toTable)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RequestInvitation(Table toTable)
        {
            try
            {
                var client = httpClientFactory.CreateClient("tableAPI");
                var result = await client.PostAsJsonAsync("/api/table/join", toTable);
                if(!result.IsSuccessStatusCode)
                {
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        #endregion
    }
}
