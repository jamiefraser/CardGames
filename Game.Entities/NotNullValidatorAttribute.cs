using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Game.Entities
{
    public class NotNullValidatorAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value ==null)
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
}
