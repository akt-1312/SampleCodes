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
    public class IsMaritalStatusUnique : ValidationAttribute, IClientModelValidator
    {

        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquemaritalstatus", "MaritalStatusName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.MaritalStatuses.ToListAsync());
            List<MaritalStatus> maritalStatuses = task.Result;
            var model = (MaritalStatusViewModel)validationContext.ObjectInstance;
            string Marital_Status = value == null ? null : value as string;
            var result = new MaritalStatus();
            if (model.MS_Id == 0)
            {
                result = maritalStatuses.Where(x => x.Marital_Status.Replace(" ", string.Empty).ToLower().Trim()
                == Marital_Status.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = maritalStatuses.Where(x => x.MS_Id != model.MS_Id && x.Marital_Status.Replace(" ", string.Empty).ToLower().Trim()
                == Marital_Status.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"MaritalStatusName {Marital_Status} already exist in current table");
        }
    }
}
