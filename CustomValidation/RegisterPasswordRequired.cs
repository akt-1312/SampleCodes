using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.CustomValidation
{
    public class RegisterPasswordRequired : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-requiredpassword",
    "The Password field is required");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            var validationModel = (RegisterViewModel)validationContext.ObjectInstance;
            if (validationModel.UserId != null)
                return ValidationResult.Success;

            var password = value as string;
            password = password == null ? null : password.Trim();
            return string.IsNullOrEmpty(password)
                ? new ValidationResult("The Password field is required.")
                : ValidationResult.Success;
            //return base.IsValid(value, validationContext);
        }
    }
}
