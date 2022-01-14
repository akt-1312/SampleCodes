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
    public class IsOnSetUnique : ValidationAttribute, IClientModelValidator
    {


        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniqueonset", "OnSetName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.OnSets.ToListAsync());
            List<OnSet> onSets = task.Result;
            var model = (OnSetViewModel)validationContext.ObjectInstance;
            string OnSetName = value == null ? null : value as string;
            var result = new OnSet();
            if (model.OnSetId == 0)
            {
                result = onSets.Where(x => x.OnSetName.Replace(" ", string.Empty).ToLower().Trim()
                == OnSetName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = onSets.Where(x => x.OnSetId != model.OnSetId && x.OnSetName.Replace(" ", string.Empty).ToLower().Trim()
                == OnSetName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"OnSetName {OnSetName} already exist in current table");
        }
    }

}
