﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.Services.SignalRService
{
    public class ReadyStateChangedEventArgs : EventArgs
    {
        public bool Ready { get; set; }
    }
}
