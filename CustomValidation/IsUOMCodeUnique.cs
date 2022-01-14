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
    public class IsUOMCodeUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniqueuomcode", "Unit Of Measurement Code {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.UnitOfMeasurements.ToListAsync());
            List<UnitOfMeasurement> uom = task.Result;
            var model = (UnitOfMeasurementViewModel)validationContext.ObjectInstance;
            string code = value == null ? null : value as string;
            var result = new UnitOfMeasurement();
            if (model.UnitOfMeasurementId == 0)
            {
                result = uom.Where(x => x.Code.Replace(" ", string.Empty).ToLower().Trim()
                == code.StringCompareFormat()
                || x.Description.Replace(" ", string.Empty).ToLower().Trim()
                == code.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = uom.Where(x => x.UnitOfMeasurementId != model.UnitOfMeasurementId && (x.Code.Replace(" ", string.Empty).ToLower().Trim()
                == code.StringCompareFormat()
                || x.Description.Replace(" ", string.Empty).ToLower().Trim()
                == code.StringCompareFormat())).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"Unit Of Measurement Code {code} already exist in current table");
        }

    }
}
