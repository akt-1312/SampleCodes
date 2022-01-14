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
    public class IsGenderUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquegender", "GenderName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.Genders.ToListAsync());
            List<Gender> genders = task.Result;
            var model = (GenderViewModel)validationContext.ObjectInstance;
            string GenderName = value == null ? null : value as string;
            var result = new Gender();
            if (model.Gender_Id == 0)
            {
                result = genders.Where(x => x.GenderName.Replace(" ", string.Empty).ToLower().Trim()
                == GenderName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = genders.Where(x => x.Gender_Id != model.Gender_Id && x.GenderName.Replace(" ", string.Empty).ToLower().Trim()
                == GenderName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"GenderName {GenderName} already exist in current table");
        }

    }
}
