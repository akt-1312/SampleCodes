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
    public class IsPrefixUnique : ValidationAttribute, IClientModelValidator
    {

        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniqueprefix", "PrefixName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.Prefixes.ToListAsync());
            List<Prefix> states = task.Result;
            var model = (PrefixViewModel)validationContext.ObjectInstance;
            string PrefixName = value == null ? null : value as string;
            var result = new Prefix();
            if (model.Prefix_Id == 0)
            {
                result = states.Where(x => x.PrefixName.Replace(" ", string.Empty).ToLower().Trim()
                == PrefixName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = states.Where(x => x.Prefix_Id != model.Prefix_Id && x.PrefixName.Replace(" ", string.Empty).ToLower().Trim()
                == PrefixName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"PrefixName {PrefixName} already exist in current table");
        }

    }
}
