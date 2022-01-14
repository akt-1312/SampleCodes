using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.CustomValidation
{
    public class RegisterRequiredRole : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-rolerequired",
    "At least one role is reqiured.");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var validationModel = (RegisterViewModel)validationContext.ObjectInstance;
            if (validationModel.UserCheckedRoles != null && validationModel.UserCheckedRoles.Count > 0)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("At least one role is reqiured.");
            }
        }
    }
}
