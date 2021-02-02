using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Refit;
namespace Game.Services.Table
{
    public interface IRTCService
    {
        [Post("/api/table/created")]
        public Task<bool> PublishTableCreatedMessage([Body] Entities.Table table);
        [Post("/api/table/joined")]
        public Task<bool> PublishTableJoinedMessage([Body] Entities.TableJoinedMessage message);
        [Post("/api/table/join")]
        public Task PublishRequestToJoinMessage([Body] Entities.RequestToJoinTableMessage message);
        [Post("/api/table/publishplayeradmitted")]
        public Task PublishPlayerAdmitted([Body] Entities.RequestToJoinTableMessage message);
    }
}
