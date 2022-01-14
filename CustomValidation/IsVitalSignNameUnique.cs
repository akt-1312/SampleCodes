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
    public class IsVitalSignNameUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquevitalsign", "VitalSign {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.VitalSignSetups.ToListAsync());
            List<VitalSignSetup> vitalSignSetups = task.Result;
            var model = (VitalSignSetUpViewModel)validationContext.ObjectInstance;
            string description = value == null ? null : value as string;
            var result = new VitalSignSetup();
            if (model.VitalSignSetupId == 0)
            {
                result = vitalSignSetups.Where(x => x.Description.Replace(" ", string.Empty).ToLower().Trim()
                == description.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = vitalSignSetups.Where(x => x.VitalSignSetupId != model.VitalSignSetupId && x.Description.Replace(" ", string.Empty).ToLower().Trim()
                == description.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"VitalSign {description} already exist in current table");
        }

    }
}
