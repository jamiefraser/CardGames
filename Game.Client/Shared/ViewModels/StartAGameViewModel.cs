using Game.Client.Shared.Services.SignalRService;
using Game.Client.Shared.ViewModels;
using Game.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Game.Client.Shared.ViewModels
{
    public class StartAGameViewModel : IStartAGameViewModel, IDisposable
    {
        private readonly ISignalRService signalRService;
        private readonly IHttpClientFactory factory;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged !=null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public StartAGameViewModel(ISignalRService _signalRService, IHttpClientFactory _factory)
        {
            signalRService = _signalRService;
            factory = _factory;
            GameTable = new Entities.Table();
            Players = new ObservableCollection<Entities.Player>();
            foreach(var p in signalRService.PlayersOnline)
            {
                Players.Add(p);
            }
            signalRService.PlayerAdded += SignalRService_PlayerAdded;
            signalRService.PlayerRemoved += SignalRService_PlayerRemoved;
            var client = factory.CreateClient("gameAPI");
            Task.Run(async () =>
            {
                Games = await client.GetFromJsonAsync<ObservableCollection<Game.Entities.Game>>("api/game");
                Console.WriteLine(Games.Count);
            });
        }

        private void SignalRService_PlayerRemoved(object sender, PlayerRemovedEventArgs e)
        {
            var p = Players.Where(player => player.PrincipalId.Equals(e.Player.PrincipalId)).FirstOrDefault();
            if (p != null) Players.Remove(p);
            RaisePropertyChanged("Players");
        }

        private void SignalRService_PlayerAdded(object sender, PlayerAddedEventArgs e)
        {
            var p = Players.Where(player => player.PrincipalId.Equals(e.Player.PrincipalId)).FirstOrDefault();
            if(p == null)
            {
                Players.Add(e.Player);
            }
            RaisePropertyChanged("Players");
        }

        public void Dispose()
        {
            signalRService.PlayerAdded -= SignalRService_PlayerAdded;
            signalRService.PlayerRemoved -= SignalRService_PlayerRemoved;
        }

        private ObservableCollection<Entities.Player> players;
        public ObservableCollection<Entities.Player> Players
        {
            get
            {
                return players;
            }
            set
            {
                players = value;
            }
        }
        private ObservableCollection<Entities.Game> games;
        public ObservableCollection<Entities.Game>Games
        {
            get
            {
                return games;
            }
            set
            {
                games = value;
                RaisePropertyChanged("Games");
            }
        }
        private Entities.Game selectedgame;
        [Required(ErrorMessage = "You must select a game to play")]
        [ValidGameValidator(ErrorMessage ="You must select a game to play")]
        public Entities.Game SelectedGame
        {
            get
            {
                return selectedgame;
            }
            set
            {
                selectedgame = value;
                RaisePropertyChanged("SelectedGame");
            }
        }
        private Entities.Table gametable;
        public Entities.Table GameTable
        {
            get
            {
                return gametable;
            }
            set
            {
                gametable = value;
                RaisePropertyChanged("GameTable");
            }
        }
    }
}
