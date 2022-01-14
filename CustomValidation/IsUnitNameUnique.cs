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
    public class IsUnitNameUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniqueunit", "Unit {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.Units.ToListAsync());
            List<Unit> units = task.Result;
            var model = (UnitViewModel)validationContext.ObjectInstance;
            string UnitName = value == null ? null : value as string;
            var result = new Unit();
            if (model.UnitId == 0)
            {
                result = units.Where(x => x.UnitName.Replace(" ", string.Empty).ToLower().Trim()
                == UnitName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = units.Where(x => x.UnitId != model.UnitId && x.UnitName.Replace(" ", string.Empty).ToLower().Trim()
                == UnitName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"Unit {UnitName} already exist in current table");
        }

    }
}
