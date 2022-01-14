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
    public class IsReactionUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquereaction", "ReactionName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.Reactions.ToListAsync());
            List<Reaction> reactions = task.Result;
            var model = (ReactionViewModel)validationContext.ObjectInstance;
            string ReactionName = value == null ? null : value as string;
            var result = new Reaction();
            if (model.ReactionId == 0)
            {
                result = reactions.Where(x => x.ReactionName.Replace(" ", string.Empty).ToLower().Trim()
                == ReactionName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = reactions.Where(x => x.ReactionId != model.ReactionId && x.ReactionName.Replace(" ", string.Empty).ToLower().Trim()
                == ReactionName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"ReactionName {ReactionName} already exist in current table");
        }
    }
}
