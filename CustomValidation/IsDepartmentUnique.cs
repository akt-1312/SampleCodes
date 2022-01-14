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
    public class IsDepartmentUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquedept", "Department {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.Departments.ToListAsync());
            List<Department> departments = task.Result;
            var model = (DepartmentViewModel)validationContext.ObjectInstance;
            string DepartmentName = value == null ? null : value as string;
            var result = new Department();
            if (model.DepartmentId == 0)
            {
                result = departments.Where(x => x.DepartmentName.Replace(" ", string.Empty).ToLower().Trim()
                == DepartmentName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = departments.Where(x => x.DepartmentId != model.DepartmentId && x.DepartmentName.Replace(" ", string.Empty).ToLower().Trim()
                == DepartmentName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"Department {DepartmentName} already exist in current table");
        }

    }
}
