using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.CustomValidation
{
    public class RequiredWhenUpdate : ValidationAttribute, IClientModelValidator
    {
        private readonly string referenceProperty;

        public RequiredWhenUpdate(string referenceProperty)
        {
            this.referenceProperty = referenceProperty;
        }
        public void AddValidation(ClientModelValidationContext context)
        {
            string displayName = context.ModelMetadata.DisplayName ?? context.ModelMetadata.PropertyName;
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-requiredwhenupdate", displayName + " Field is required");
            context.Attributes.Add("data-val-referenceproperty", "#" + referenceProperty);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var refProperty = validationContext.ObjectInstance.GetType().GetProperty(referenceProperty);
            var refPropertyValue = refProperty.GetValue(validationContext.ObjectInstance, null);

            string requiredValue = value == null ? null : value.ToString();
            string refValue = refPropertyValue == null ? null : refPropertyValue.ToString();

            if (string.IsNullOrWhiteSpace(refValue))
                return ValidationResult.Success;

            string displayName = validationContext.DisplayName;
            return string.IsNullOrWhiteSpace(requiredValue) || requiredValue.ToString().Trim() == "0" 
                ? new ValidationResult(displayName + " Field is required")
                    : ValidationResult.Success;
        }
    }
}
