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
    public class IsCountryUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquecountry", "CountryName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.Countries.ToListAsync());
            List<Country> country = task.Result;
            var model = (CountriesViewModel)validationContext.ObjectInstance;
            string Cty_name = value == null ? null : value as string;
            var result = new Country();
            if (model.Cty_id == 0)
            {
                result = country.Where(x => x.Cty_name.Replace(" ", string.Empty).ToLower().Trim()
                == Cty_name.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = country.Where(x => x.Cty_id != model.Cty_id && x.Cty_name.Replace(" ", string.Empty).ToLower().Trim()
                == Cty_name.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"CountryName {Cty_name} already exist in current table");
        }
    }
}
