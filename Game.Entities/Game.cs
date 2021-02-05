using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Game.Entities
{
    //[TypeConverter(typeof(StringToGameTypeConverter))]
    public class Game : Microsoft.Azure.Cosmos.Table.TableEntity
    {
        private int _numberofdecks = 1;
        [Required]
        [Display(Name = "How Many Decks?")]
        [GreaterThanZeroValidator(ErrorMessage ="You must specify at least one deck")]
        public int NumberOfDecks
        {
            get
            {
                return _numberofdecks;
            }
            set
            {
                if (value < 1) return;
                _numberofdecks = value;
            }
        }
        private DeckType decktype;
        [Required]
        [EnumDataType(enumType: typeof(DeckType), ErrorMessage = "Please select a valid deck type")]
        [JsonProperty]
        public DeckType DeckType
        {
            get
            {
                return decktype;
            }
            set
            {
                decktype = value;
            }
        }

        [Required(AllowEmptyStrings =false, ErrorMessage = "Please provide a minimum number of players")]
        [GreaterThanZeroValidator(ErrorMessage = "Please set a minimum number of players that is greater than zero")]
        [Display(Name = "Minimum Number of Players")]
        public int MinimumPlayers { get; set; }

        [Required()]
        [GreaterThanZeroValidator(ErrorMessage = "Please set a maximmum number of players that is greater than zero")]
        [Display(Name = "Maximum Number of Players")]
        public int MaxPlayers { get; set; }

        [Required]
        [Display(Name = "Use a Discard Pile?")]
        private bool _usesdiscardpile = false;

        public bool UsesDiscardPile
        {
            get
            {
                return _usesdiscardpile;
            }
            set
            {
                if(value==false)
                {
                    PlayerPicksUpWholeDiscardPile = false;
                }
                _usesdiscardpile = value;
            }
        }

        [Display(Name = "Pick up Whole Discard Pile?")]
        public bool PlayerPicksUpWholeDiscardPile { get; set; }

        [Display(Name = "How Many Cards in a Hand?")]
        [Required]
        [GreaterThanZeroValidator(ErrorMessage ="Number of cards to deal must be greater than zero")]
        public int NumberOfCardsToDeal { get; set; }

        private bool _progressivedeal;

        [Display(Name = "Each Hand Gets More Cards?")]
        public bool ProgressiveDeal 
        {
            get
            {
                return _progressivedeal;
            }
            set
            {
                if(value==false)
                {
                    IncrementCardsToDealBy = 0;
                }
                _progressivedeal = value;
            }
        }

        [Display(Name = "How Many More Cards per Hand?")]
        public int IncrementCardsToDealBy { get; set; }
        
        private string _name;
        [Required]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RowKey = value;
            }
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
