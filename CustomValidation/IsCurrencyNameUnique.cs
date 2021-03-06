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
    public class IsCurrencyNameUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquecurrency", "Currency {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.Currencies.ToListAsync());
            List<Currency> currencies = task.Result;
            var model = (CurrencyViewModel)validationContext.ObjectInstance;
            string CurrencyName = value == null ? null : value as string;
            var result = new Currency();
            if (model.CurrencyId == 0)
            {
                result = currencies.Where(x => x.CurrencyName.Replace(" ", string.Empty).ToLower().Trim()
                == CurrencyName.StringCompareFormat()
                || x.CurrencyCode.Replace(" ", string.Empty).ToLower().Trim()
                == CurrencyName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = currencies.Where(x => x.CurrencyId != model.CurrencyId && (x.CurrencyName.Replace(" ", string.Empty).ToLower().Trim()
                == CurrencyName.StringCompareFormat()
                || x.CurrencyCode.Replace(" ", string.Empty).ToLower().Trim()
                == CurrencyName.StringCompareFormat())).FirstOrDefault();
            }
            //if (model.CurrencyId == 0)
            //{
            //    result = currencies.Where(x => x.CurrencyName.Replace(" ", string.Empty).ToLower().Trim()
            //    == CurrencyCode.StringCompareFormat()).FirstOrDefault();
            //}
            //else
            //{
            //    result = currencies.Where(x => x.CurrencyId != model.CurrencyId && x.CurrencyCode.Replace(" ", string.Empty).ToLower().Trim()
            //    == CurrencyCode.StringCompareFormat()).FirstOrDefault();
            //}
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"Currency Name {CurrencyName} already exist in current table");
        }


    }
}
