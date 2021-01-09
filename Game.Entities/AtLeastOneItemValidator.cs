using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Game.Entities
{
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
}
