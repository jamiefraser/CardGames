using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Entities
{
    public class PresenceStatusMessage
    {
        public PlayerPresence CurrentStatus { get; set; }
        public Player Player { get; set; }
        public bool InAGame { get; set; }
    }
    public enum PlayerPresence
    {
        Online,
        Offline
    }
}
