﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Client.Shared.Services.SignalRService
{
    public class TableRemovedEventArgs : EventArgs
    {
        public Entities.Table Table { get; set; }
    }
}
