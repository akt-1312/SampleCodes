using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Administration;
using HMS.Models.ViewModels.Menu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Controllers.Menu
{
    [Area("Menu")]
    [Authorize]
    public class ModuleMenuController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext db;

        public ModuleMenuController(UserManager<ApplicationUser> userManager,
                                    SignInManager<ApplicationUser> signInManager,
                                    RoleManager<IdentityRole> roleManager,
                                    ApplicationDbContext db)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.db = db;
        }

        [HttpGet]
        public IActionResult ChooseRole(string model)
        {
            UserChooseRoleViewModel chooseRoleVm = new UserChooseRoleViewModel();
            if (model != null)
            {
                chooseRoleVm = Newtonsoft.Json.JsonConvert.DeserializeObject<UserChooseRoleViewModel>(model);
            }
            return View(chooseRoleVm);
        }

        [HttpGet]
        public async Task<IActionResult> BackToRoleSelect()
        {
            if (signInManager.IsSignedIn(User) && (Request.Cookies["LoginRole"] != null && Request.Cookies["IsPersistentRole"] != null))
            {
                try
                {
                    var test = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    ApplicationUser user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    var rolesOfUser = await userManager.GetRolesAsync(user);
                    UserChooseRoleViewModel model = new UserChooseRoleViewModel();
                    model.Roles = rolesOfUser.ToList();
                    model.IsPersistent = bool.Parse(Request.Cookies["IsPersistentRole"]);
                    var modelString = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                    return RedirectToAction(nameof(ChooseRole), new { model = modelString });
                }
                catch
                {
                    return View("Error");
                }
            }
            try { 
            await signInManager.SignOutAsync();
            Response.Cookies.Delete("LoginRole");
            Response.Cookies.Delete("IsPersistentRole");
            return RedirectToAction("Login", "UserAccount", new { area = "Administration" });
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpGet]
        public IActionResult UserRoleSelect(string role, bool isPersistent)
        {
            CookieOptions options = new CookieOptions();
            if (isPersistent)
            {
                options.Expires = DateTime.Now.AddDays(30);
            }

            Response.Cookies.Append("LoginRole", role, options);
            //Response.Cookies.Append("IsPersistentRole", isPersistent ? "true" : "false", options);

            return RedirectToAction(nameof(MainMenu));
        }

        [HttpGet]
        public async Task<IActionResult> MainMenu()
        {

            if (signInManager.IsSignedIn(User) && (Request.Cookies["LoginRole"] != null && Request.Cookies["IsPersistentRole"] != null))
            {
                return View();
            }
            else
            {
                if (signInManager.IsSignedIn(User) && Request.Cookies["IsPersistentRole"] != null)
                {
                    try { 
                    var test = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    ApplicationUser user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    var rolesOfUser = await userManager.GetRolesAsync(user);
                    UserChooseRoleViewModel model = new UserChooseRoleViewModel();
                    model.Roles = rolesOfUser.ToList();
                    model.IsPersistent = bool.Parse(Request.Cookies["IsPersistentRole"]);
                    var modelString = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                    return RedirectToAction(nameof(ChooseRole), new { model = modelString });
                    }
                    catch
                    {
                        return View("Error");
                    }
                }
                else
                {
                    try { 
                    await signInManager.SignOutAsync();
                    Response.Cookies.Delete("LoginRole");
                    Response.Cookies.Delete("IsPersistentRole");
                    return RedirectToAction("Login", "UserAccount", new { area = "Administration" });
                    }
                    catch
                    {
                        return View("Error");
                    }
                }
            }

        }
    }
}