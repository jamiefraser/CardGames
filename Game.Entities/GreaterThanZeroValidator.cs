using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Game.Entities
{
    public class GreaterThanZeroValidator : ValidationAttribute
    {
        public static ValidationResult IsGreaterThanZero(DeckType value)
        {
            ValidationResult result = new ValidationResult(((DeckType)value) > 0 ? null : "Not greater than zero");
            return result ; 
        }
    }
}
