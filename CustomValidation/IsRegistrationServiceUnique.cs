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
    public class IsRegistrationServiceUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquers", "Registration Service {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.RegistrationServices.ToListAsync());
            List<RegistrationService> rs = task.Result;
            var model = (RegistrationServiceViewModel)validationContext.ObjectInstance;
            string RegistrationServiceDescription = value == null ? null : value as string;
            var result = new RegistrationService();
            if (model.RegistrationServiceId == 0)
            {
                result = rs.Where(x => x.RegistrationServiceDescription.Replace(" ", string.Empty).ToLower().Trim()
                == RegistrationServiceDescription.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = rs.Where(x => x.RegistrationServiceId != model.RegistrationServiceId && x.RegistrationServiceDescription.Replace(" ", string.Empty).ToLower().Trim()
                == RegistrationServiceDescription.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"Registration Service {RegistrationServiceDescription} already exist in current table");
        }

    }
}
