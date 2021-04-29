using System;
using System.Collections.Generic;
#if CLIENT
using System.ComponentModel.DataAnnotations;
#endif
using System.Text;

namespace Game.Entities
{
#if CLIENT
    public class ValidGameValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value==null)
            {
                return new ValidationResult(this.ErrorMessage);
            }
            var game = value as Entities.Game;
            if (game == null)
            {
                return new ValidationResult(this.ErrorMessage);
            }
            if(string.IsNullOrEmpty(game.Name))
            {
                return new ValidationResult(this.ErrorMessage);
            }
            else
            {
                return ValidationResult.Success;
            }
        }
        public static ValidationResult IsGreaterThanZero(int value)
        {
            ValidationResult result = new ValidationResult(((DeckType)value) > 0 ? null : "Not greater than zero");
            return result;
        }
    }
#endif
}
