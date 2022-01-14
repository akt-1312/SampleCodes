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
    public class IsRelationshipUnique : ValidationAttribute, IClientModelValidator
    {
        public void AddValidation(ClientModelValidationContext context)
        {
            //context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-isuniquerelationship", "RelationshipName {0} already exist in current table");
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var db = (ApplicationDbContext)validationContext
                         .GetService(typeof(ApplicationDbContext));
            var task = Task.Run(async () => await db.Relationships.ToListAsync());
            List<Relationship> relationships = task.Result;
            var model = (RelationshipViewModel)validationContext.ObjectInstance;
            string RelationshipName = value == null ? null : value as string;
            var result = new Relationship();
            if (model.RelationshipId == 0)
            {
                result = relationships.Where(x => x.RelationshipName.Replace(" ", string.Empty).ToLower().Trim()
                == RelationshipName.StringCompareFormat()).FirstOrDefault();
            }
            else
            {
                result = relationships.Where(x => x.RelationshipId != model.RelationshipId && x.RelationshipName.Replace(" ", string.Empty).ToLower().Trim()
                == RelationshipName.StringCompareFormat()).FirstOrDefault();
            }
            if (result == null)
                return ValidationResult.Success;
            return new ValidationResult($"RelationshipName {RelationshipName} already exist in current table");
        }

    }
}
