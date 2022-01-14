using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.CustomValidation
{
    public class RegisterConfirmPassword : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-comparedpassword",
    "Confirm Password must match Password.");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            var validationModel = (RegisterViewModel)validationContext.ObjectInstance;
            if (validationModel.UserId != null)
                return ValidationResult.Success;

            var confirmPassword = value as string;
            var password = validationModel.Password;
            confirmPassword = confirmPassword == null ? confirmPassword : confirmPassword.Trim();
            password = password == null ? null : password.Trim();
            return confirmPassword != password
                ? new ValidationResult("Confirm Password must match Password.")
                : ValidationResult.Success;
            //return base.IsValid(value, validationContext);
        }
    }
}
