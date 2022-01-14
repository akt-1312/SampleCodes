using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Administration;
using HMS.Models.SuperAdmin;
using HMS.Models.ViewModels;
using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize]
    public class PageNamesInRoleController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly RoleManager<IdentityRole> roleManager;

        public PageNamesInRoleController(ApplicationDbContext db,
                                         RoleManager<IdentityRole> roleManager)
        {
            this.db = db;
            this.roleManager = roleManager;
        }

        [Authorize(Policy = "AdminRoleMenu")]
        [HttpGet]
        public async Task<IActionResult> PageNamesInRole()
        {
            List<RoleAndPageViewModal> roleAndPageViewModals = new List<RoleAndPageViewModal>();
            List<PageNamesInModule> pageNamesInModules = new List<PageNamesInModule>();
            List<IdentityRole> identityRoles = new List<IdentityRole>();

            pageNamesInModules = await db.PageNamesInModules.Include(x => x.Module).ToListAsync();
            identityRoles = await roleManager.Roles.ToListAsync();
            var pagesInRole = await db.PagesInRoles.Include(x => x.IdentityRole).Include(x => x.PageNamesInModule).ToListAsync();


            foreach (var role in identityRoles.OrderBy(x => x.Name))
            {
                List<PageAndModuleViewModel> pageAndModuleViewModels = new List<PageAndModuleViewModel>();
                List<PageGroupByModuleViewModel> pageGroupByModuleViewModels = new List<PageGroupByModuleViewModel>();
                foreach (var item in pageNamesInModules)
                {
                    PageAndModuleViewModel pageAndModuleViewModel = new PageAndModuleViewModel();
                    var existPageInRole = pagesInRole.Where(x => x.RoleId == role.Id && x.Status
                    && x.PageNamesInModuleId == item.PageNamesInModuleId)
                                .FirstOrDefault();

                    pageAndModuleViewModel.Role = role;
                    pageAndModuleViewModel.PageNamesInModule = item;
                    pageAndModuleViewModel.IsChecked = existPageInRole == null ? false : true;
                    pageAndModuleViewModel.IsCreateAccess = existPageInRole == null ? false : existPageInRole.IsCreateAccess;
                    pageAndModuleViewModel.IsUpdateAccess = existPageInRole == null ? false : existPageInRole.IsUpdateAccess;
                    pageAndModuleViewModel.IsDeleteAccess = existPageInRole == null ? false : existPageInRole.IsDeleteAccess;

                    pageAndModuleViewModels.Add(pageAndModuleViewModel);
                }

                pageGroupByModuleViewModels = pageAndModuleViewModels.GroupBy(x => x.PageNamesInModule.Module.ModuleName)
                                               .Select(m => new PageGroupByModuleViewModel()
                                               {
                                                   ModuleName = m.Key,
                                                   //ModuleId = m.ToList().Select(x=> x.PageNamesInModule.ModuleId).FirstOrDefault(),
                                                   PageNamesInModules = m.ToList()
                                               }).ToList();
                roleAndPageViewModals.Add(new RoleAndPageViewModal()
                {
                    Role = role,
                    PageGroupsByModules = pageGroupByModuleViewModels
                });

            }
            bool isSaveSuccess = false;
            if (TempData["IsSaveSuccess"] != null)
            {
                bool.TryParse(TempData["IsSaveSuccess"].ToString(), out isSaveSuccess);
            }
            ViewBag.IsSaveSuccess = isSaveSuccess;
            return View(roleAndPageViewModals);

        }

        [Authorize(Policy = "AdminRoleMenu")]
        [HttpPost]
        public async Task<IActionResult> PageNamesInRole(List<PagesInRoleUpdateViewModal> modals)
        {
            try
            {
                var pageNameInRoleList = await db.PagesInRoles.ToListAsync() ?? new List<PagesInRole>();
                foreach (var item in modals)
                {
                    PagesInRole newPageInRole = new PagesInRole();
                    var pageNameInRole = pageNameInRoleList.Where(x => x.RoleId == item.RoleId
                    && x.PageNamesInModuleId == item.PageNamesInModuleId).FirstOrDefault();
                    if (pageNameInRole == null)
                    {
                        if (item.IsCheck)
                        {
                            newPageInRole.RoleId = item.RoleId;
                            newPageInRole.PageNamesInModuleId = item.PageNamesInModuleId;
                            newPageInRole.Status = item.IsCheck;
                            newPageInRole.IsCreateAccess = item.IsCreateAccess;
                            newPageInRole.IsUpdateAccess = item.IsUpdateAccess;
                            newPageInRole.IsDeleteAccess = item.IsDeleteAccess;
                            await db.PagesInRoles.AddAsync(newPageInRole);
                        }
                    }
                    else
                    {
                        if (item.IsCheck)
                        {
                            if (pageNameInRole.Status != item.IsCheck || pageNameInRole.IsCreateAccess != item.IsCreateAccess
                                || pageNameInRole.IsUpdateAccess != item.IsUpdateAccess || pageNameInRole.IsDeleteAccess != item.IsDeleteAccess)
                            {
                                pageNameInRole.Status = item.IsCheck;
                                pageNameInRole.IsCreateAccess = item.IsCreateAccess;
                                pageNameInRole.IsUpdateAccess = item.IsUpdateAccess;
                                pageNameInRole.IsDeleteAccess = item.IsDeleteAccess;

                                db.PagesInRoles.Update(pageNameInRole);
                            }
                        }
                        else
                        {
                            db.PagesInRoles.Remove(pageNameInRole);
                        }
                    }
                }

                await db.SaveChangesAsync();
                TempData["IsSaveSuccess"] = true;
                return RedirectToAction(nameof(PageNamesInRole));
            }
            catch (Exception)
            {
                return View("Error");
            }


            //try
            //{
            //    var pageNameInRoleList = await db.PagesInRoles.ToListAsync() ?? new List<PagesInRole>();
            //    foreach (var item in modals)
            //    {
            //        PagesInRole newPageInRole = new PagesInRole();
            //        var pageNameInRole = pageNameInRoleList.Where(x => x.RoleId == item.RoleId
            //        && x.PageNamesInModuleId == item.PageNamesInModuleId).FirstOrDefault();
            //        if (pageNameInRole == null)
            //        {
            //            newPageInRole.RoleId = item.RoleId;
            //            newPageInRole.PageNamesInModuleId = item.PageNamesInModuleId;
            //            newPageInRole.Status = item.IsCheck;
            //            newPageInRole.IsCreateAccess = item.IsCreateAccess;
            //            newPageInRole.IsUpdateAccess = item.IsUpdateAccess;
            //            newPageInRole.IsDeleteAccess = item.IsDeleteAccess;

            //            await db.PagesInRoles.AddAsync(newPageInRole);
            //        }
            //        else
            //        {
            //            if (pageNameInRole.Status != item.IsCheck || pageNameInRole.IsCreateAccess != item.IsCreateAccess 
            //                || pageNameInRole.IsUpdateAccess != item.IsUpdateAccess || pageNameInRole.IsDeleteAccess != item.IsDeleteAccess)
            //            {
            //                pageNameInRole.Status = item.IsCheck;
            //                pageNameInRole.IsCreateAccess = item.IsCreateAccess;
            //                pageNameInRole.IsUpdateAccess = item.IsUpdateAccess;
            //                pageNameInRole.IsDeleteAccess = item.IsDeleteAccess;

            //                db.PagesInRoles.Update(pageNameInRole);
            //            }
            //        }
            //    }

            //    await db.SaveChangesAsync();
            //    TempData["IsSaveSuccess"] = true;
            //    return RedirectToAction(nameof(PageNamesInRole));
            //}
            //catch (Exception)
            //{
            //    return View("Error");
            //}
        }
    }
}