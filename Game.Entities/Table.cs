using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Game.Entities
{
    public class Table
    {
        public EasyAuthUserInfo TableOwner { get; set; }
        public ObservableCollection<Player> Players { get; set; }
        private bool _finished;
        public bool Finished
        {
            get
            {
                return _finished;
            }
            set
            {
                _finished = value;
                if (value == true)
                {
                    //Call the deck service to delete any associated decks
                    //store the scores for each player
                    //blow away the record for this table
                }
            }
        }
        public string Name { get; set; }
        public int MaxPlayers { get; private set; }
        private Game _game;
        public Game Game
        {
            get
            {
                return _game;
            }
            set
            {
                _game = value;
                MaxPlayers = _game.MaxPlayers;
            }
        }
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
            this.Finished = false;
        }
    }
}
