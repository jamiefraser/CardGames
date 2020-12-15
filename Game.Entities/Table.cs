using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Game.Entities
{
    public class Table : TableEntity
    {
        string SerializedTableOwner { get; set; }
        string SerializedPlayers { get; set; }
        public EasyAuthUserInfo TableOwner { get; set; }
        [IgnoreProperty]
        public ObservableCollection<Player> Players { get; set; }
        public bool Finished { get; set; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; }
        public int NumberOfDecks { get; set; }
        public int NumberOfCardsInAHand { get; set; }
        public Game Game { get; set; }
        public int CurrentPlayersCount
        {
            get
            {
                return Players.Count;
            }
        }
        public Table()
        {
            this.Players = new ObservableCollection<Player>();
        }
    }
}
