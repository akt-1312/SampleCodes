using HMS.Models.ViewModels.Nurse;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.CustomValidation
{
    public class AllergyRequiredFieldWhenAllergyExist : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            string displayName = context.ModelMetadata.DisplayName ?? context.ModelMetadata.PropertyName;
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-allergyfieldsrequired", displayName + " Field is required");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (AllergicViewModel)validationContext.ObjectInstance;
            if (model.IsNoKnownAllergy)
                return ValidationResult.Success;

            string displayName = validationContext.DisplayName;
            var requiredValue = value == null ? null : value.ToString();
            return string.IsNullOrWhiteSpace(requiredValue) ? new ValidationResult(displayName + " Field is required")
                    : ValidationResult.Success;
        }

    }
}
