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
    public class IsReferalHospitalUnique : ValidationAttribute, IClientModelValidator
    {

        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquehospital", "HospitalName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.ReferalHospitals.ToListAsync());
            List<ReferalHospital> referalHospitals = task.Result;
            var model = (ReferalHospitalViewModel)validationContext.ObjectInstance;
            string HospitalName = value == null ? null : value as string;
            var result = new ReferalHospital();
            if (model.Id == 0)
            {
                result = referalHospitals.Where(x => x.HospitalName.Replace(" ", string.Empty).ToLower().Trim()
                == HospitalName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = referalHospitals.Where(x => x.Id != model.Id && x.HospitalName.Replace(" ", string.Empty).ToLower().Trim()
                == HospitalName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"HospitalName {HospitalName} already exist in current table");
        }
    }
}
