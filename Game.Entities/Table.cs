using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if CLIENT
using System.ComponentModel.DataAnnotations;
#endif
using System.Text;

namespace Game.Entities
{
    public class Table
    {
        public List<Entities.Card>DealtCards
        { get; set; }
        public Guid Id { get; set; }
        public EasyAuthUserInfo TableOwner { get; set; }

        private Player dealer;
        public Player Dealer
        {
            get
            {
                return dealer;
            }
            set
            {
                dealer = value;
            }
        }
        public List<Player> InvitedPlayers
        {
            get;
            set;
        }
        public string[] InvitedPlayerIds { get; set; }
        public SortedList<int,Player> Players
        {
            get;
            set;
        }
        private bool _started;
        public bool Started
        {
            get
            {
                return _started;
            }
            set
            {
                _started = value;
            }
        }
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
        private List<Player> playersrequestingaccess;
        public List<Player> PlayersRequestingAccess
        {
            get
            {
                return playersrequestingaccess;
            }
            set
            {
                playersrequestingaccess = value;
            }
        }
        public string Name { get; set; }
        public int MaxPlayers { get; private set; }
        private Game _game;
#if CLIENT
        [NotNullValidator(ErrorMessage = "You must select a game to play")]
#endif
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
                if (_game.DeckType.Equals(DeckType.Standard))
                {
                    Deck = new StandardDeck();
                    DiscardPile = new StandardDeck();
                    DiscardPile.Cards = new Stack<Card>();
                }
                if (_game.DeckType.Equals(DeckType.Phase10))
                {
                    Deck = new Phase10Deck();
                    DiscardPile = new Phase10Deck();
                    DiscardPile.Cards = new Stack<Card>();
                }
            }
        }
        public int CurrentPlayersCount
        {
            get
            {
                return Players.Count;
            }
        }

        private DeckBase deck;
        [JsonProperty]
        public DeckBase Deck
        {
            get
            {
                return deck;
            }
            set
            {
                deck = value;
            }
        }

        private DeckBase discardpile;
        public DeckBase DiscardPile
        {
            get
            {
                return discardpile;
            }
            set
            {
                discardpile = value;
            }
        }
        public Table()
        {
            this.Players = new SortedList<int,Player>();
            InvitedPlayers = new List<Player>();
            this.Finished = false;
            PlayersRequestingAccess = new List<Player>();
            DealtCards = new List<Card>();
        }
    }
}
