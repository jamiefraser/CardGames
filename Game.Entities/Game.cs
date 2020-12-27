using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Game.Entities
{
    public class Game : Microsoft.Azure.Cosmos.Table.TableEntity
    {
        [Required]
        [Display(Name ="How Many Decks?")]
        public int NumberOfDecks { get; set; }
        [Required]
        [EnumDataType(enumType:typeof(DeckType), ErrorMessage = "Please select a valid deck type")]
        public DeckType DeckType { get; set; }
        [Display(Name ="Minimum Number of Players")]
        [Required]
        public int MinimumPlayers { get; set; }
        [Required()]
        [Display(Name ="Maximum Number of Players")]
        public int MaxPlayers { get; set; }
        [Required]
        [Display(Name ="Use a Discard Pile?")]
        public bool UsesDiscardPile { get; set; }
        [Display(Name ="Pick up Whole Discard Pile?")]
        public bool PlayerPicksUpWholeDiscardPile { get; set; }
        [Display(Name ="How Many Cards in a Hand?")]
        [Required]
        public int NumberOfCardsToDeal { get; set; }
        [Display(Name ="Each Hand Gets More Cards?")]
        public bool ProgressiveDeal { get; set; }
        [Display(Name ="How Many More Cards per Hand?")]
        public int IncrementCardsToDealBy { get; set; }
        [Required]
        [Display(Name ="Name")]
        public new string RowKey { get; set; }
    }
}
