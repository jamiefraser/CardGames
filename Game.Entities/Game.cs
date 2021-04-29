using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
#if CLIENT
using System.ComponentModel.DataAnnotations;
#endif
using System.ComponentModel;

namespace Game.Entities
{
    //[TypeConverter(typeof(StringToGameTypeConverter))]
    public class Game : Microsoft.Azure.Cosmos.Table.TableEntity
    {
        private int _numberofdecks = 1;
#if CLIENT
        [Required]
        [Display(Name = "How Many Decks?")]
        [GreaterThanZeroValidator(ErrorMessage ="You must specify at least one deck")]
#endif
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
#if CLIENT
        [Required]
        [EnumDataType(enumType: typeof(DeckType), ErrorMessage = "Please select a valid deck type")]
#endif
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
#if CLIENT
        [Required(AllowEmptyStrings =false, ErrorMessage = "Please provide a minimum number of players")]
        [GreaterThanZeroValidator(ErrorMessage = "Please set a minimum number of players that is greater than zero")]
        [Display(Name = "Minimum Number of Players")]
#endif
        public int MinimumPlayers { get; set; }
#if CLIENT
        [Required()]
        [GreaterThanZeroValidator(ErrorMessage = "Please set a maximmum number of players that is greater than zero")]
        [Display(Name = "Maximum Number of Players")]
#endif
        public int MaxPlayers { get; set; }
#if CLIENT
        [Required]
        [Display(Name = "Use a Discard Pile?")]
#endif
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
#if CLIENT
        [Display(Name = "Pick up Whole Discard Pile?")]
#endif
        public bool PlayerPicksUpWholeDiscardPile { get; set; }
#if CLIENT
        [Display(Name = "How Many Cards in a Hand?")]
        [Required]
        [GreaterThanZeroValidator(ErrorMessage ="Number of cards to deal must be greater than zero")]
#endif
        public int NumberOfCardsToDeal { get; set; }

        private bool _progressivedeal;
#if CLIENT
        [Display(Name = "Each Hand Gets More Cards?")]
#endif
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
#if CLIENT
        [Display(Name = "How Many More Cards per Hand?")]
#endif
        public int IncrementCardsToDealBy { get; set; }
        
        private string _name;
#if CLIENT
        [Required]
#endif
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
