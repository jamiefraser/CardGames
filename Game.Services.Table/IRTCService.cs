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
    }
}
