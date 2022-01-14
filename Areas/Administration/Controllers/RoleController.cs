using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.SuperAdmin.Controllers
{
    [Area("Administration")]
    [Authorize]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IAuthorizationService authorizationService;
        private readonly ApplicationDbContext db;

        public RoleController(RoleManager<IdentityRole> roleManager,
                                IAuthorizationService authorizationService,
                                ApplicationDbContext db)
        {
            this.roleManager = roleManager;
            this.authorizationService = authorizationService;
            this.db = db;
        }

        [Authorize(Policy = "RoleView")]
        [HttpGet, ActionName("Role")]
        public async Task<IActionResult> Role()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            CreateRoleViewModel model = new CreateRoleViewModel();
            model.IdentityRoles = await roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
            return View(model);
        }

        [Authorize(Policy = "RoleView")]
        [HttpPost, ActionName("Role")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RolePost(CreateRoleViewModel model, string btnSubmit, string roleId)
        {
            btnSubmit = btnSubmit.ToLower().Trim();
            model.IdentityRoles = await roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "RoleDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }
                ModelState.Clear();

                model = new CreateRoleViewModel();
                IdentityRole identityRole = new IdentityRole();
                try
                {
                    identityRole = await roleManager.FindByIdAsync(roleId);
                    if (identityRole != null)
                    {
                        IdentityResult result = await roleManager.DeleteAsync(identityRole);
                        if (result.Succeeded)
                        {
                            TempData["SuccessedAlertMessage"] = "Delete Successful";
                            return RedirectToAction(nameof(Role));
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            return View(model);
                        }
                    }
                    else
                    {
                        ViewBag.ErrorTitle = $"RoleId {roleId} not found";
                        ViewBag.ErrorMessage = $"Cannot find RoleId {roleId}." +
                            $"Please Contact IT Team";
                        return View("Error");
                    }
                }
                catch (DbUpdateException)
                {
                    ViewBag.ErrorTitle = $"{identityRole.Name} role is in use";
                    ViewBag.ErrorMessage = $"{identityRole.Name} role cannot be deleted as there are users in thes role." +
                        $" If you want to delete this role, please remove the user from the role and then try to delete";
                    return View("Error");
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (btnSubmit == "create")
                    {
                        if (!(await authorizationService.AuthorizeAsync(User, "RoleCreate")).Succeeded)
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", "Administration");
                        }
                        try
                        {
                            IdentityRole identityRole = new IdentityRole
                            {
                                Name = model.Name.Trim(),
                            };
                            IdentityResult result = await roleManager.CreateAsync(identityRole);
                            if (result.Succeeded)
                            {
                                TempData["SuccessedAlertMessage"] = "Save Successful";
                                return RedirectToAction(nameof(Role));
                            }
                            else
                            {
                                foreach (var error in result.Errors)
                                {
                                    ModelState.AddModelError("", error.Description);
                                }
                            }
                        }
                        catch
                        {
                            return View("Error");
                        }

                    }
                    if (btnSubmit == "update")
                    {
                        if (!(await authorizationService.AuthorizeAsync(User, "RoleUpdate")).Succeeded)
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", "Administration");
                        }
                        try
                        {
                            IdentityRole identityRole = await roleManager.FindByIdAsync(model.Id);
                            if (identityRole != null)
                            {
                                identityRole.Name = model.Name.Trim();
                                IdentityResult result = await roleManager.UpdateAsync(identityRole);

                                if (result.Succeeded)
                                {
                                    TempData["SuccessedAlertMessage"] = "Update Successful";
                                    return RedirectToAction(nameof(Role));
                                }
                                else
                                {
                                    foreach (var error in result.Errors)
                                    {
                                        ModelState.AddModelError("", error.Description);
                                    }
                                }
                            }
                            else
                            {
                                return View("Error");
                            }
                        }
                        catch
                        {
                            return View("Error");
                        }


                    }
                }
                return View(model);
            }
        }

        //[HttpPost]
        //[Authorize(Policy = "RoleDelete")]
        //public async Task<IActionResult> DeleteRole(string roleId)
        //{

        //}
    }
}