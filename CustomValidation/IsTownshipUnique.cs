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
    public class IsTownshipUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquetownship", "TownshipName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.Townships.ToListAsync());
            List<Township> townships = task.Result;
            var model = (TownshipViewModel)validationContext.ObjectInstance;
            string Tsp_name = value == null ? null : value as string;
            var result = new Township();
            if (model.Tsp_id == 0)
            {
                result = townships.Where(x => x.Tsp_name.Replace(" ", string.Empty).ToLower().Trim()
                == Tsp_name.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = townships.Where(x => x.Tsp_id != model.Tsp_id && x.Tsp_name.Replace(" ", string.Empty).ToLower().Trim()
                == Tsp_name.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"TownshipName {Tsp_name} already exist in current table");
        }
    }
}
