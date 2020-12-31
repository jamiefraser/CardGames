using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Game.Client.Client.Services.SignalRService
{
    public interface ISignalRService
    {
        string AccessToken { get; set; }
    }
}
