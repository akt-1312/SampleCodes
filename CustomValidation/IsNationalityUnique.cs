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
    public class IsNationalityUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquenationality", "NationalityName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.Nationalities.ToListAsync());
            List<Nationality> nationalities = task.Result;
            var model = (NationalityViewModel)validationContext.ObjectInstance;
            string NationalityName = value == null ? null : value as string;
            var result = new Nationality();
            if (model.Nation_Id == 0)
            {
                result = nationalities.Where(x => x.NationalityName.Replace(" ", string.Empty).ToLower().Trim()
                == NationalityName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = nationalities.Where(x => x.Nation_Id != model.Nation_Id && x.NationalityName.Replace(" ", string.Empty).ToLower().Trim()
                == NationalityName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"NationalityName {NationalityName} already exist in current table");
        }
    }
}
