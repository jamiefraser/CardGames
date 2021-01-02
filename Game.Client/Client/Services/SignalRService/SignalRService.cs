using Game.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Game.Client.Client.Services.SignalRService
{
    public class SignalRService : ISignalRService
    {
        private HubConnection hubConnection;
        private List<string> messages = new List<string>();
        public SignalRService()
        {
            hubConnection = new HubConnectionBuilder().WithUrl("http://localhost:5864/api/").Build();

            Task.Run(async () =>
            {
                await hubConnection.StartAsync();
                hubConnection.On<string>("broadcast", (message) =>
                {
                    var encodedMsg = $"{message}";
                    Console.WriteLine(encodedMsg);
                    messages.Add(encodedMsg);
                });
                hubConnection.On<PresenceStatusMessage>("presence", (message) =>
                {
                    Console.WriteLine($"{message.Player.PrincipalName} {(message.CurrentStatus.Equals(PlayerPresence.Online) ? " is online" : " is offline")}");
                });
            });
            Players = new List<Entities.Player>();
        }

        

        public string AccessToken
        {
            get;
            set;
        }
        public List<Entities.Player> Players { get; set; }
    }
}
