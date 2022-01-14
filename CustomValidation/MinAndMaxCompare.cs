using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.CustomValidation
{
    public class MinAndMaxCompare : ValidationAttribute, IClientModelValidator
    {
        private readonly bool isForMax;
        private readonly string comparedProperty;

        public MinAndMaxCompare(bool isForMax, string comparedProperty)
        {
            this.isForMax = isForMax;
            this.comparedProperty = comparedProperty;
        }
        public void AddValidation(ClientModelValidationContext context)
        {
            string errorMessage = isForMax ? "Must be greater than Minumum Value" : "Must be less than Maximum Value"; 
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-comparedminmax", errorMessage);
            context.Attributes.Add("data-val-comparedproperty", "#" + comparedProperty);
            context.Attributes.Add("data-val-ismax", isForMax ? "true" : "false");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectInstance.GetType().GetProperty(comparedProperty);
            var propertyValue = property.GetValue(validationContext.ObjectInstance, null);
            string strValue = value == null ? null : value.ToString();
            string strPropertyValue = propertyValue == null ? null : propertyValue.ToString();
            float mainValue;
            float comparedValue;
            if(value == null || !float.TryParse(strValue, out mainValue) || propertyValue == null || !float.TryParse(strPropertyValue, out comparedValue))
                return ValidationResult.Success;

            mainValue = float.Parse(strValue);
            comparedValue = float.Parse(strPropertyValue);
            if (isForMax)
            {
                return mainValue > comparedValue ? ValidationResult.Success :
                    new ValidationResult("Must be greater than Minumum Value");
            }
            else
            {
                return mainValue < comparedValue ? ValidationResult.Success :
                    new ValidationResult("Must be less than Maximum Value");
            }
        }
    }
}
