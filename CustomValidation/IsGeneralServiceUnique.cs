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
    public class IsGeneralServiceUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquegs", "General Service {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.GeneralServices.ToListAsync());
            List<GeneralService> gs = task.Result;
            var model = (GeneralServiceViewModel)validationContext.ObjectInstance;
            string GeneralServiceDescription = value == null ? null : value as string;
            var result = new GeneralService();
            if (model.GeneralServiceId == 0)
            {
                result = gs.Where(x => x.GeneralServiceDescription.Replace(" ", string.Empty).ToLower().Trim()
                == GeneralServiceDescription.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = gs.Where(x => x.GeneralServiceId != model.GeneralServiceId && x.GeneralServiceDescription.Replace(" ", string.Empty).ToLower().Trim()
                == GeneralServiceDescription.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"General Service {GeneralServiceDescription} already exist in current table");
        }

    }
}
