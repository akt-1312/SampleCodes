using HMS.Models.Reception;
using HMS.Models.ViewModels.Reception;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.CustomValidation
{
    public class PatientRegRemarkRequiredWhenUpdate : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-remarkrequirewhenupdate", "The Remark Field is required.");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var model = (PatientRegistration)validationContext.ObjectInstance;
            if (model.Reg_Id == 0)
                return ValidationResult.Success;

            string remark = value == null ? null : value.ToString();
            return string.IsNullOrWhiteSpace(remark) ? new ValidationResult("The Remark Field is required.")
                    : ValidationResult.Success;
        }
    }
}
