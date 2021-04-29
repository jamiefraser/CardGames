using System;
using System.Collections.Generic;
#if CLIENT
using System.ComponentModel.DataAnnotations;
#endif
using System.Text;

namespace Game.Entities
{
#if CLIENT
    public class AtLeastOneItemValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult(this.ErrorMessage);
            }
            if((value as Array).Length==0)
            {
                return new ValidationResult(this.ErrorMessage);
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }
#endif
}
