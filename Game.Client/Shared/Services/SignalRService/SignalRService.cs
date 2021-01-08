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
namespace Game.Client.Shared.Services.SignalRService
{
    public class SignalRService : ISignalRService
    {
        private HubConnection hubConnection;
        private List<string> messages = new List<string>();
        private IHttpClientFactory _factory;
        public SignalRService(IHttpClientFactory factory, string serviceBaseUrl)
        {
            _factory = factory;
            var clientAddress = factory.CreateClient("PresenceServiceRoot").BaseAddress;
            hubConnection = new HubConnectionBuilder().WithUrl($"{serviceBaseUrl}api").Build();

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
                        if (PlayersOnline.Contains(message.Player)) return;
                        var p = new Player()
                        {
                            ETag = message.Player.ETag,
                            Hand = message.Player.Hand,
                            PartitionKey = message.Player.PartitionKey,
                            PrincipalId = message.Player.PrincipalId,
                            PrincipalIdp = message.Player.PrincipalIdp,
                            PrincipalName = message.Player.PrincipalName,
                            RowKey = message.Player.RowKey,
                            Timestamp = message.Player.Timestamp
                        };
                        
                        PlayersOnline.Add(p);
                        RaisePlayerAdded(p);
                    }
                    else
                    {
                        var p = PlayersOnline.Where(po => po.PrincipalId.Equals(message.Player.PrincipalId)).FirstOrDefault();
                        if(p!=null)PlayersOnline.Remove(p);
                        RaisePlayerRemoved(p);
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

        public event EventHandler<PlayerAddedEventArgs> PlayerAdded;
        public event EventHandler<PlayerRemovedEventArgs> PlayerRemoved;
        private void RaisePlayerAdded(Entities.Player player)
        {
            if(PlayerAdded != null)
            {
                PlayerAdded(this,new PlayerAddedEventArgs()
                {
                    Player = player
                });
            }
        }
        private void RaisePlayerRemoved(Entities.Player player)
        {
            if(PlayerRemoved != null)
            {
                PlayerRemoved(this, new PlayerRemovedEventArgs()
                {
                    Player = player
                });
            }
        }
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
