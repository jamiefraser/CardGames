using System;
using System.Collections.Generic;
#if CLIENT
using System.ComponentModel.DataAnnotations;
#endif
using System.Text;

namespace Game.Entities
{
#if CLIENT
    public class GreaterThanZeroValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            int? testValue = value as int?;
            if(testValue != null && testValue.HasValue && testValue.Value > 0)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(this.ErrorMessage);
            }
            return base.IsValid(value, validationContext);
        }
        public static ValidationResult IsGreaterThanZero(int value)
        {
            ValidationResult result = new ValidationResult(((DeckType)value) > 0 ? null : "Not greater than zero");
            return result ; 
        }
    }
#endif
}
