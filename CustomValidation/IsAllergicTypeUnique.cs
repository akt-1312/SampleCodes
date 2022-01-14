using HMS.Data;
using HMS.Extensions;
using HMS.Models.Administration;
using HMS.Models.ViewModels;
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
    public class IsAllergicTypeUnique : ValidationAttribute, IClientModelValidator
    {

        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniqueallergictype", "AllergicType {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.AllergicTypes.ToListAsync());
            List<AllergicType> allergicTypes = task.Result;
            var model = (AllergicTypeViewModel)validationContext.ObjectInstance;
            string AllergicTypeName = value == null ? null : value as string;
            var result = new AllergicType();
            if (model.AllergicTypeId == 0)
            {
                result = allergicTypes.Where(x => x.AllergicTypeName.Replace(" ", string.Empty).ToLower().Trim()
                == AllergicTypeName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = allergicTypes.Where(x => x.AllergicTypeId != model.AllergicTypeId && x.AllergicTypeName.Replace(" ", string.Empty).ToLower().Trim()
                == AllergicTypeName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"AllergicType {AllergicTypeName} already exist in current table");
        }

    }
}
