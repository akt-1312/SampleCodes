using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models;
using HMS.Models.Administration;
using HMS.Models.ViewModels;
using HMS.Models.ViewModels.Administration;
using HMS.Models.ViewModels.Menu;
using HMS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HMS.Areas.SuperAdmin.Controllers
{
    [Area("Administration")]
    [Authorize]
    public class UserAccountController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IAuthorizationService authorizationService;

        public RegisterViewModel RegisterVM { get; set; }
        public UserAccountController(ApplicationDbContext db,
                                      UserManager<ApplicationUser> userManager,
                                      SignInManager<ApplicationUser> signInManager,
                                      RoleManager<IdentityRole> roleManager,
                                      IAuthorizationService authorizationService)
        {
            this.db = db;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.authorizationService = authorizationService;
        }

        [HttpGet]
        [Authorize(Policy = "AccountRegisterView")]
        public async Task<IActionResult> AccountRegister()
        {
            RegisterViewModel model = new RegisterViewModel();
            var roles = await roleManager.Roles.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();
            model.Roles = roles ?? new List<string>();
            var users = await userManager.Users.OrderBy(x => x.UserName).ToListAsync();
            foreach (var item in users)
            {
                RolesInRegisterUser rolesInRegisterUser = new RolesInRegisterUser();
                rolesInRegisterUser.User = new AccountRegisterUserData()
                {
                    UserId = item.Id,
                    UserName = item.UserName,
                    Email = item.Email,
                    PhoneNo = item.PhoneNumber,
                    Password = item.PasswordHash,
                    ConfirmPassword = item.PasswordHash,
                };
                List<string> allRolesOfUser = (await userManager.GetRolesAsync(item)).ToList();
                rolesInRegisterUser.Roles = allRolesOfUser;
                rolesInRegisterUser.AllRolesToDisplay = string.Join(", ", allRolesOfUser);
                model.RolesInRegisterUsers.Add(rolesInRegisterUser);
            }
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "AccountRegisterView")]
        public async Task<IActionResult> AccountRegister(RegisterViewModel model, string btnSubmit, string userId)
        {
            btnSubmit = btnSubmit.ToLower().Trim();

            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "AccountRegisterDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }
                ModelState.Clear();
                try
                {
                    model = new RegisterViewModel();
                    var user = await userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return View("Error");
                    }
                    model.Roles = await roleManager.Roles.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();
                    model.RolesInRegisterUsers = new List<RolesInRegisterUser>();
                    var users = await userManager.Users.OrderBy(x => x.UserName).ToListAsync();
                    foreach (var item in users)
                    {
                        RolesInRegisterUser rolesInRegisterUser = new RolesInRegisterUser();
                        rolesInRegisterUser.User = new AccountRegisterUserData()
                        {
                            UserId = item.Id,
                            UserName = item.UserName,
                            Email = item.Email,
                            PhoneNo = item.PhoneNumber,
                            Password = item.PasswordHash,
                            ConfirmPassword = item.PasswordHash,
                        };
                        List<string> allRolesOfUser = (await userManager.GetRolesAsync(item)).ToList();
                        rolesInRegisterUser.Roles = allRolesOfUser;
                        rolesInRegisterUser.AllRolesToDisplay = string.Join(", ", allRolesOfUser);
                        model.RolesInRegisterUsers.Add(rolesInRegisterUser);
                    }
                    if (user.Email.Trim() == SD.initialEmail.Trim())
                    {
                        ModelState.AddModelError("", "This user cannot be deleted because of special user!");
                        return View(model);
                    }
                    var roles = await userManager.GetRolesAsync(user);
                    var result = await userManager.RemoveFromRolesAsync(user, roles.ToArray());

                    if (result.Succeeded)
                    {
                        var userRemoveResult = await userManager.DeleteAsync(user);
                        if (userRemoveResult.Succeeded)
                        {
                            TempData["SuccessedAlertMessage"] = "User Account Successfully Deleted";
                            return RedirectToAction(nameof(AccountRegister));
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
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }
                }
                catch (Exception)
                {
                    return View("Error");
                }

            }
            else
            {
                model.Roles = await roleManager.Roles.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();
                model.RolesInRegisterUsers = new List<RolesInRegisterUser>();
                var users = await userManager.Users.OrderBy(x => x.UserName).ToListAsync();
                foreach (var item in users)
                {
                    RolesInRegisterUser rolesInRegisterUser = new RolesInRegisterUser();
                    rolesInRegisterUser.User = new AccountRegisterUserData()
                    {
                        UserId = item.Id,
                        UserName = item.UserName,
                        Email = item.Email,
                        PhoneNo = item.PhoneNumber,
                        Password = item.PasswordHash,
                        ConfirmPassword = item.PasswordHash,
                    };
                    List<string> allRolesOfUser = (await userManager.GetRolesAsync(item)).ToList();
                    rolesInRegisterUser.Roles = allRolesOfUser;
                    rolesInRegisterUser.AllRolesToDisplay = string.Join(", ", allRolesOfUser);
                    model.RolesInRegisterUsers.Add(rolesInRegisterUser);
                }
                if (ModelState.IsValid)
                {
                    if (btnSubmit == "create")
                    {
                        if ((await authorizationService.AuthorizeAsync(User, "AccountRegisterCreate")).Succeeded)
                        {
                            ApplicationUser applicationUser = new ApplicationUser()
                            {
                                UserName = model.UserName.Trim(),
                                Email = model.Email.Trim(),
                                PhoneNumber = model.PhoneNo.Trim()
                            };
                            var result = await userManager.CreateAsync(applicationUser, model.Password.Trim());

                            if (result.Succeeded)
                            {
                                for (int i = 0; i < model.UserCheckedRoles.Count; i++)
                                {
                                    var isRoleExist = model.Roles.Contains(model.UserCheckedRoles[i]);
                                    if (isRoleExist)
                                    {
                                        var resultRole = await userManager.AddToRoleAsync(applicationUser, model.UserCheckedRoles[i]);
                                        if (resultRole.Succeeded)
                                        {
                                            if (i < model.UserCheckedRoles.Count - 1)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                TempData["SuccessedAlertMessage"] = "User Account Successfully Registered";
                                                return RedirectToAction(nameof(AccountRegister));
                                            }
                                        }
                                    }

                                }
                                TempData["SuccessedAlertMessage"] = "User Account Successfully Registered";
                                return RedirectToAction(nameof(AccountRegister));
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
                            return RedirectToAction(nameof(AccessDenied));
                        }
                    }
                    else
                    {
                        if ((await authorizationService.AuthorizeAsync(User, "AccountRegisterUpdate")).Succeeded)
                        {
                            ApplicationUser editUser = await userManager.FindByIdAsync(model.UserId);
                            if (editUser == null)
                            {
                                return View("Error");
                            }
                            else if (editUser.Email.Trim() == SD.initialEmail && model.Email.Trim() != SD.initialEmail.Trim())
                            {

                                ModelState.AddModelError("", "This user email cannot be changed because of policy restricted!");
                                return View(model);

                            }
                            else
                            {
                                editUser.UserName = model.UserName;
                                editUser.Email = model.Email;
                                editUser.PhoneNumber = model.PhoneNo;

                                var result = await userManager.UpdateAsync(editUser);
                                if (result.Succeeded)
                                {
                                    try
                                    {
                                        var oldUserRole = await db.UserRoles.Where(x => x.UserId == editUser.Id).ToListAsync();
                                        db.UserRoles.RemoveRange(oldUserRole);

                                        List<IdentityUserRole<string>> identityUserRoles = new List<IdentityUserRole<string>>();
                                        var allIdentityRoles = await roleManager.Roles.ToListAsync();
                                        foreach (var role in model.UserCheckedRoles)
                                        {
                                            IdentityUserRole<string> identityUserRole = new IdentityUserRole<string>();
                                            identityUserRole.UserId = editUser.Id;
                                            identityUserRole.RoleId = allIdentityRoles.Where(x => x.Name.Trim() == role.Trim()).Select(x => x.Id).FirstOrDefault();
                                            identityUserRoles.Add(identityUserRole);
                                        }

                                        await db.UserRoles.AddRangeAsync(identityUserRoles);
                                        await db.SaveChangesAsync();

                                        TempData["SuccessedAlertMessage"] = "User Account Successfully Updated";
                                        return RedirectToAction(nameof(AccountRegister));
                                    }
                                    catch (Exception)
                                    {
                                        return View("Error");
                                    }
                                }
                                else
                                {
                                    foreach (var error in result.Errors)
                                    {
                                        ModelState.AddModelError("", error.Description);
                                    }
                                }
                            }
                        }
                        else
                        {
                            return RedirectToAction(nameof(AccessDenied));
                        }
                    }
                    return View(model);
                }
                else
                {
                    return View(model);
                }
            }
        }


        //[HttpPost]
        //[Authorize(Policy = "AccountRegisterDelete")]
        //public async Task<IActionResult> DeleteAccountRegister(string userId)
        //{

        //}

        //#region OldRegister
        //[Authorize(Policy = "AccountRegisterView")]
        //[HttpGet, ActionName("AccountRegister")]
        //public async Task<IActionResult> AccountRegister(string id)
        //{
        //    if (id == null)
        //    {
        //        RegisterVM = new RegisterViewModel
        //        {
        //            RegisterList = await db.Users.OrderByDescending(x => x.Id).ToListAsync(),
        //        };
        //        ViewData["RoleData"] = await db.Roles.ToListAsync();
        //        ViewData["UserRole"] = await db.UserRoles.ToListAsync();
        //        return View(RegisterVM);
        //    }
        //    else
        //    {
        //        if ((await authorizationService.AuthorizeAsync(User, "AccountRegisterUpdate")).Succeeded ||
        //            (await authorizationService.AuthorizeAsync(User, "AccountRegisterDelete")).Succeeded)
        //        {
        //            var user = await db.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
        //            RegisterVM = new RegisterViewModel
        //            {
        //                Id = user.Id,
        //                UserName = user.UserName,
        //                Email = user.Email,
        //                PhoneNo = user.PhoneNumber,
        //                RegisterList = await db.Users.OrderByDescending(x => x.Id).ToListAsync(),
        //            };
        //            var selectUserId = await db.UserRoles.Where(m => m.UserId == user.Id).ToListAsync();
        //            ViewData["UserRoleList"] = selectUserId;
        //            ViewData["RoleData"] = await db.Roles.ToListAsync();
        //            ViewData["UserRole"] = await db.UserRoles.ToListAsync();
        //            return View(RegisterVM);
        //        }
        //        else
        //        {
        //            return RedirectToAction(nameof(AccessDenied));
        //        }

        //    }
        //}

        //[Authorize(Policy = "AccountRegisterView")]
        //[HttpPost, ActionName("AccountRegister")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AccountRegisterPost(string id, RegisterViewModel regVm)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if(regVm.CheckData == null || regVm.CheckData.Count == 0)
        //        {
        //            ModelState.AddModelError("CheckData", "At least one role must assign");
        //            var selectUserId = await db.UserRoles.Where(m => m.UserId == regVm.Id).ToListAsync();
        //            ViewData["UserRoleList"] = selectUserId;
        //            ViewData["RoleData"] = await db.Roles.ToListAsync();
        //            ViewData["UserRole"] = await db.UserRoles.ToListAsync();
        //            regVm.RegisterList = await db.Users.OrderByDescending(x => x.Id).ToListAsync();
        //            return View(regVm);
        //        }
        //        if (id == null || id == "0")
        //        {
        //            if ((await authorizationService.AuthorizeAsync(User, "AccountRegisterCreate")).Succeeded)
        //            {
        //                var registerUser = new ApplicationUser()
        //                {
        //                    UserName = regVm.UserName.Trim(),
        //                    Email = regVm.Email.Trim(),
        //                    PhoneNumber = regVm.PhoneNo.Trim(),
        //                };
        //                var result = await userManager.CreateAsync(registerUser, regVm.Password);

        //                if (registerUser != null)
        //                {
        //                    foreach (var item in regVm.CheckData)
        //                    {
        //                        result = await userManager.AddToRoleAsync(registerUser, item);
        //                    }
        //                }

        //                if (result.Succeeded)
        //                {
        //                    //await signInManager.SignInAsync(registerUser, isPersistent: false);
        //                    //return RedirectToAction("Login");
        //                    var routeValues = new RouteValueDictionary {
        //                                    { "id", null },
        //                                  };
        //                    return RedirectToAction("AccountRegister", "UserAccount", routeValues);
        //                }
        //                foreach (var error in result.Errors)
        //                {
        //                    ModelState.AddModelError("", error.Description);
        //                }
        //            }
        //            else
        //            {
        //                return RedirectToAction(nameof(AccessDenied));
        //            }

        //        }
        //        else
        //        {
        //            if ((await authorizationService.AuthorizeAsync(User, "AccountRegisterUpdate")).Succeeded)
        //            {
        //                var userUpdate = await db.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
        //                userUpdate.UserName = regVm.UserName.Trim();
        //                userUpdate.Email = regVm.Email.Trim();
        //                userUpdate.PhoneNumber = regVm.PhoneNo.Trim();
        //                await db.SaveChangesAsync();

        //                var deleteRoleUser = await db.UserRoles.Where(m => m.UserId == userUpdate.Id).ToListAsync();
        //                db.RemoveRange(deleteRoleUser);
        //                await db.SaveChangesAsync();
        //                if (userUpdate != null)
        //                {
        //                    foreach (var item in regVm.CheckData)
        //                    {
        //                        var result = await userManager.AddToRoleAsync(userUpdate, item);
        //                    }
        //                }
        //                var routeValues = new RouteValueDictionary {
        //                                    { "id", null },
        //                                  };
        //                return RedirectToAction("AccountRegister", "UserAccount", routeValues);
        //            }
        //            else
        //            {
        //                return RedirectToAction(nameof(AccessDenied));
        //            }
        //        }
        //    }

        //    RegisterVM = new RegisterViewModel
        //    {
        //        RegisterList = await db.Users.OrderByDescending(x => x.Id).ToListAsync(),
        //    };
        //    ViewData["RoleData"] = await db.Roles.ToListAsync();
        //    ViewData["UserRole"] = await db.UserRoles.ToListAsync();
        //    return View(RegisterVM);
        //}

        //public async Task<IActionResult> Delete(string id)
        //{
        //    if ((await authorizationService.AuthorizeAsync(User, "AccountRegisterDelete")).Succeeded)
        //    {
        //        try
        //        {
        //            var deleteRoleUser = db.UserRoles.Where(m => m.UserId == id).ToList();
        //            db.RemoveRange(deleteRoleUser);
        //            db.SaveChanges();
        //            var user = await db.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
        //            db.Remove(user);
        //            await db.SaveChangesAsync();
        //        }
        //        catch
        //        {
        //            ViewBag.ErrorTitle = "These user is used in another table";
        //        }
        //        return Json("success");
        //    }
        //    else
        //    {
        //        return RedirectToAction(nameof(AccessDenied));
        //    }
        //}
        //#endregion

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            Response.Cookies.Delete("LoginRole");
            Response.Cookies.Delete("IsPersistentRole");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoadingApplication()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            if (signInManager.IsSignedIn(User) && (Request.Cookies["LoginRole"] != null && Request.Cookies["IsPersistentRole"] != null))
            {
                return RedirectToAction("MainMenu", "ModuleMenu", new { area = "Menu" });
                //ApplicationUser user = await userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                //var rolesOfUser = await userManager.GetRolesAsync(user);
                //if (rolesOfUser.Count > 1)
                //{
                //    return RedirectToAction("MainMenu", "ModuleMenu", new { area = "Menu" });

                //    //if(Request.Cookies["LoginRole"] == null || Request.Cookies["IsPersistentRole"] == null)
                //    //{
                //    //    //UserChooseRoleViewModel model = new UserChooseRoleViewModel();
                //    //    //model.Roles = rolesOfUser.ToList();
                //    //    //model.IsPersistent = bool.Parse(Request.Cookies["IsPersistentRole"]);
                //    //    //var modelString = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                //    //    //return RedirectToAction("ChooseRole", "ModuleMenu", new { area = "Menu", model = modelString });
                //    //}
                //    //else
                //    //{
                //    //    return RedirectToAction("MainMenu", "ModuleMenu", new { area = "Menu" });
                //    //}
                //}
                //else
                //{
                //    CookieOptions options = new CookieOptions();
                //    if (bool.Parse(Request.Cookies["IsPersistentRole"]))
                //    {
                //        options.Expires = DateTime.Now.AddDays(30);
                //    }

                //    Response.Cookies.Append("LoginRole", rolesOfUser.FirstOrDefault(), options);
                //    Response.Cookies.Append("IsPersistentRole", bool.Parse(Request.Cookies["IsPersistentRole"]) ? "true" : "false", options);

                //    return RedirectToAction("MainMenu", "ModuleMenu", new { area = "Menu" });
                //        //return RedirectToAction("MainMenuPage", "HmsMenu", new { area = "Menu" });

                //}
                //return RedirectToAction("MainMenuPage", "HmsMenu", new { area = "Menu" });
                //return RedirectToAction(nameof(AccountRegister));
            }
            await signInManager.SignOutAsync();
            Response.Cookies.Delete("LoginRole");
            Response.Cookies.Delete("IsPersistentRole");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel loginVm, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var loginUser = await userManager.FindByEmailAsync(loginVm.Email);
                    if (loginUser != null)
                    {
                        var result = await signInManager.PasswordSignInAsync(loginUser, loginVm.Password, loginVm.RememberMe, false);

                        if (result.Succeeded)
                        {
                            CookieOptions options = new CookieOptions();
                            if (loginVm.RememberMe)
                            {
                                options.Expires = DateTime.Now.AddDays(30);
                            }

                            Response.Cookies.Append("IsPersistentRole", loginVm.RememberMe ? "true" : "false", options);

                            var rolesOfUser = await userManager.GetRolesAsync(loginUser);
                            if (rolesOfUser.Count > 1)
                            {
                                UserChooseRoleViewModel model = new UserChooseRoleViewModel();
                                model.Roles = rolesOfUser.ToList();
                                model.IsPersistent = loginVm.RememberMe;
                                var modelString = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                                return RedirectToAction("ChooseRole", "ModuleMenu", new { area = "Menu", model = modelString });
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                {
                                    //CookieOptions options = new CookieOptions();
                                    if (loginVm.RememberMe)
                                    {
                                        options.Expires = DateTime.Now.AddDays(30);
                                    }

                                    Response.Cookies.Append("LoginRole", rolesOfUser.FirstOrDefault(), options);
                                    //Response.Cookies.Append("IsPersistentRole", loginVm.RememberMe ? "true" : "false", options);
                                    return Redirect(returnUrl);
                                }
                                else
                                {
                                    //CookieOptions options = new CookieOptions();
                                    if (loginVm.RememberMe)
                                    {
                                        options.Expires = DateTime.Now.AddDays(30);
                                    }

                                    Response.Cookies.Append("LoginRole", rolesOfUser.FirstOrDefault(), options);
                                    //Response.Cookies.Append("IsPersistentRole", loginVm.RememberMe ? "true" : "false", options);
                                    return RedirectToAction("MainMenu", "ModuleMenu", new { area = "Menu" });
                                    //return RedirectToAction(nameof(AccountRegister));
                                    //return RedirectToAction("MainMenuPage", "HmsMenu", new { area = "Menu" });
                                }
                            }

                        }
                        ModelState.AddModelError("", "Invalid Login Attempt");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid Login Attempt");
                    }
                }
                catch (Exception)
                {
                    return View("Error");
                }
            }

            return View(loginVm);
        }
    }
}