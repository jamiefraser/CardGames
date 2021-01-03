using Game.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Json;
namespace Game.Client.Client.Services.SignalRService
{
    public class SignalRService : ISignalRService
    {
        private HubConnection hubConnection;
        private List<string> messages = new List<string>();
        private IHttpClientFactory _factory;
        public SignalRService(IHttpClientFactory factory)
        {
            _factory = factory;
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
                    if(message.CurrentStatus.Equals(PlayerPresence.Online))
                    {
                        PlayersOnline.Add(message.Player);
                    }
                    else
                    {
                        var p = PlayersOnline.Where(po => po.PrincipalId.Equals(message.Player.PrincipalId)).FirstOrDefault();
                        PlayersOnline.Remove(p);
                    }
                });
            });
            var client = factory.CreateClient("presenceAPI");
            Task.Run(async () =>
            {
                var players = await client.GetFromJsonAsync<List<Entities.Player>>("api/players");
                PlayersOnline = new ObservableCollection<Entities.Player>(players);
            });
        }

        private ObservableCollection<Entities.Player> _playersonline;
        public ObservableCollection<Entities.Player>PlayersOnline
        {
            get
            {
                return _playersonline;
            }
            set
            {
                _playersonline = value;
            }
        }

        public string AccessToken
        {
            get;
            set;
        }
    }
}
