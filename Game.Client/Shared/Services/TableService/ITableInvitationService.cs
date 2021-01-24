using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.Services.TableInvitationService
{
    public interface ITableInvitationService
    {
        public Task<bool> RequestInvitation(Entities.Table toTable);
        public void CancelRequestForInvitation(Entities.Table toTable);
    }
}
