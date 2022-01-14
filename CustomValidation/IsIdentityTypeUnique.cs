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
    public class IsIdentityTypeUnique : ValidationAttribute, IClientModelValidator
    {

        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniqueidentity", "IdentityTypeName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.IdentityTypes.ToListAsync());
            List<IdentityType> identityTypes = task.Result;
            var model = (IdentityTypeViewModel)validationContext.ObjectInstance;
            string IdentityTypeName = value == null ? null : value as string;
            var result = new IdentityType();
            if (model.IdentityTypeId == 0)
            {
                result = identityTypes.Where(x => x.IdentityTypeName.Replace(" ", string.Empty).ToLower().Trim()
                == IdentityTypeName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = identityTypes.Where(x => x.IdentityTypeId != model.IdentityTypeId && x.IdentityTypeName.Replace(" ", string.Empty).ToLower().Trim()
                == IdentityTypeName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"IdentityTypeName {IdentityTypeName} already exist in current table");
        }
    }
}
