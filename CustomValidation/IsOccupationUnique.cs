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
    public class IsOccupationUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniqueoccupation", "OccupationName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.Occupations.ToListAsync());
            List<Occupation> occupations  = task.Result;
            var model = (OccupationViewModel)validationContext.ObjectInstance;
            string OccupationName = value == null ? null : value as string;
            var result = new Occupation();
            if (model.Occu_Id == 0)
            {
                result = occupations.Where(x => x.OccupationName.Replace(" ", string.Empty).ToLower().Trim()
                == OccupationName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = occupations.Where(x => x.Occu_Id != model.Occu_Id && x.OccupationName.Replace(" ", string.Empty).ToLower().Trim()
                == OccupationName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"OccupationName {OccupationName} already exist in current table");
        }

    }
}
