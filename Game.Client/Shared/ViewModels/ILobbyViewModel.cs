using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.ViewModels
{
    public interface ILobbyViewModel
    {
        Entities.Table Table { get; set; }

        void Initialize(Guid tableId);
    }
}
