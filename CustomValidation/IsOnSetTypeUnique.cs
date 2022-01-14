using HMS.Data;
using HMS.Extensions;
using HMS.Models.Administration;
using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.CustomValidation
{
    public class IsOnSetTypeUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniqueonsettype", "OnSetTypeName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.OnSetTypes.ToListAsync());
            List<OnSetType> onSetTypes  = task.Result;
            var model = (OnSetTypeViewModel)validationContext.ObjectInstance;
            string OnSetTypeName = value == null ? null : value as string;
            var result = new OnSetType();
            if (model.OnSetTypeId == 0)
            {
                result = onSetTypes.Where(x => x.OnSetTypeName.Replace(" ", string.Empty).ToLower().Trim()
                == OnSetTypeName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = onSetTypes.Where(x => x.OnSetTypeId != model.OnSetTypeId && x.OnSetTypeName.Replace(" ", string.Empty).ToLower().Trim()
                == OnSetTypeName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"OnSetTypeName {OnSetTypeName} already exist in current table");
        }
    }
}
