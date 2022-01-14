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
    public class IsStateUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquestate", "StateName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.States.ToListAsync());
            List<State> states = task.Result;
            var model = (StateViewModel)validationContext.ObjectInstance;
            string State_name = value == null ? null : value as string;
            var result = new State();
            if (model.State_id == 0)
            {
                result = states.Where(x => x.State_name.Replace(" ", string.Empty).ToLower().Trim()
                == State_name.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = states.Where(x => x.State_id != model.State_id && x.State_name.Replace(" ", string.Empty).ToLower().Trim()
                == State_name.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"StateName {State_name} already exist in current table");
        }

    }
}
