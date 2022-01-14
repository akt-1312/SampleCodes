using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Administration;
using HMS.Models.Enums;
using HMS.Models.Reception;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Areas.CommonAjaxCall.Controllers
{
    [Area("CommonAjaxCall")]
    [AllowAnonymous]
    public class RemoteCustomValidationController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;

        public RemoteCustomValidationController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public IActionResult IsRequiredMobile([FromQuery(Name = "patientRegistration.RegistrationType")] RegistrationType registrationType, [FromQuery(Name = "patientRegistration.PatientMobile1")] string mobile)
        {
            if (string.IsNullOrEmpty(mobile))
            {
                if (registrationType == RegistrationType.Emergency)
                {
                    return Json(true);
                }
                else
                {
                    return Json("Mobile field is required");
                }
            }
            else
            {
                return Json(true);
            }
        }

        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public IActionResult IsRequiredDOB([FromQuery(Name = "patientRegistration.RegistrationType")] RegistrationType registrationType, [FromQuery(Name = "patientRegistration.DOB")] string dob)
        {
            if (string.IsNullOrEmpty(dob))
            {
                if (registrationType == RegistrationType.Emergency)
                {
                    return Json(true);
                }
                else
                {
                    return Json("Date of Birth field is required");
                }
            }
            else
            {
                return Json(true);
            }
        }

        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public IActionResult IsRequiredAge([FromQuery(Name = "patientRegistration.RegistrationType")] RegistrationType registrationType, [FromQuery(Name = "patientRegistration.Age")] string age)
        {
            if (string.IsNullOrEmpty(age))
            {
                if (registrationType == RegistrationType.Emergency)
                {
                    return Json(true);
                }
                else
                {
                    return Json("Age field is required");
                }
            }
            else
            {
                return Json(true);
            }
        }

        [AcceptVerbs("Get, Post")]
        [AllowAnonymous]
        public IActionResult IsZeroFormulaRate(float formulaRate)
        {
            if (formulaRate != 0)
            {
                return Json(true);
            }
            else
            {
                return Json("Formula Rate must not zero");
            }
        }

        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email, string id)
        {
            var userById = await userManager.FindByIdAsync(id);
            var user = await userManager.FindByEmailAsync(email);
            if (userById == null)
            {
                if (user == null)
                {
                    return Json(true);
                }
                return Json($"Email {email} is already in use.");
            }
            else
            {
                if (user != userById)
                {
                    if (user == null)
                    {
                        return Json(true);
                    }
                    else
                    {
                        return Json($"Email {email} is already in use.");
                    }
                }
                else
                {
                    return Json(true);
                }
            }
        }

        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public IActionResult AccountRegisterOrUpdatePasswordRequiredToggle(string password, string userId)
        {
            password = password == null ? password : password.Trim();
            userId = userId == null ? userId : userId.Trim();
            if (string.IsNullOrEmpty(userId))
            {
                if (string.IsNullOrEmpty(password))
                {
                    return Json("Password field is required");
                }
                else
                {
                    return Json(true);
                }
            }
            else
            {
                return Json(true);
            }
        }

        //[AcceptVerbs("Get", "Post")]
        //[AllowAnonymous]
        //public IActionResult ComparePasswordToggle(string confirmPassword, string userId, string password)
        //{
        //    password = password == null ? password : password.Trim();
        //    confirmPassword = confirmPassword == null ? confirmPassword : confirmPassword.Trim();
        //    userId = userId == null ? userId : userId.Trim();
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        if(password == confirmPassword)
        //        {
        //            return Json(true);
        //        }
        //        else
        //        {
        //            return Json("Password and ConfirmPassword must match");
        //        }
        //    }
        //    else
        //    {
        //        return Json(true);
        //    }
        //}
    }
}
