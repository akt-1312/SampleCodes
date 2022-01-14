using HMS.Models.Enums;
using HMS.Models.Reception;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.CustomValidation
{
    public class RequiredWhenNormalRegType : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-requiredwhennormalregtype", "The " + context.ModelMetadata.GetDisplayName() + " Field is required");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (PatientRegistration)validationContext.ObjectInstance;
            if (model.RegistrationType == RegistrationType.Emergency)
                return ValidationResult.Success;

            return value == null || string.IsNullOrWhiteSpace(value.ToString()) ? new ValidationResult("The " + validationContext.DisplayName + " Field is required")
                : ValidationResult.Success;

        }
    }
}
