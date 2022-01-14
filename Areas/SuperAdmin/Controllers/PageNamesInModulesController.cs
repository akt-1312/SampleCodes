using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models.Administration;
using HMS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using HMS.Models.SuperAdmin;

namespace HMS.Areas.SuperAdmin.Controllers
{
    [Authorize]
    [Area("SuperAdmin")]
    public class PageNamesInModulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService authorizationService;
        public PageNamesInModulesViewModel PageNamesInModulesVM { get; set; }
        public PageNamesInModulesController(ApplicationDbContext context,
                                            IAuthorizationService authorizationService)
        {
            _context = context;
            this.authorizationService = authorizationService;
            PageNamesInModulesVM = new PageNamesInModulesViewModel()
            {
                Modules = new List<Module>(),
                PageNamesInModules = new List<PageNamesInModule>(),
            };
        }
        [HttpGet]
        [Authorize(Policy = "PageNamesInModulesView")]
        public async Task<IActionResult> PageNamesInModules()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            PageNamesInModulesViewModel model = new PageNamesInModulesViewModel();
            var totalpg = _context.PageNamesInModules.Include(x => x.Module).OrderBy(a => a.PageName).ToList();
            model.Modules =await _context.Modules.OrderBy(x => x.ModuleName).ToListAsync();
            model.PageNamesInModules = totalpg;
            return View(model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "UnitView")]
        public async Task<IActionResult> PageNamesInModules(PageNamesInModulesViewModel model, string btnSubmit)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalpg = _context.PageNamesInModules.Include(x => x.Module).OrderBy(a => a.PageName).ToList();
            var total = await _context.PageNamesInModules.OrderBy(a => a.PageName).ToListAsync();
            model.Modules = await _context.Modules.OrderBy(x => x.ModuleName).ToListAsync();
            model.PageNamesInModules = totalpg;
            PageNamesInModule pg = new PageNamesInModule();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (ModelState.IsValid)
            {
                //var NameExist = await _context.Townships.Where(x => x.Tsp_name == model.Township.Tsp_name).FirstOrDefaultAsync();
                if (btnSubmit == "update")
                {
                    if ((await authorizationService.AuthorizeAsync(User, "PageNamesInModulesUpdate")).Succeeded)
                    {
                        try
                        {
                            if (model.PageNamesInModuleId == null || model.PageNamesInModuleId.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await PageExists(PageName: model.PageName, PageNamesInModuleId: model.PageNamesInModuleId))
                                {
                                    ModelState.AddModelError("PageName", "Page Name already exist");
                                    return View(model);

                                }
                                else
                                {
                                    var updated = await _context.PageNamesInModules.FindAsync(model.PageNamesInModuleId.Value);
                                    if (updated != null)
                                    {
                                        updated.PageName = model.PageName;
                                        updated.ModuleId = model.ModuleId;
                                        updated.UpdatedDate = UpdatedDate;
                                        _context.PageNamesInModules.Update(updated);
                                        await _context.SaveChangesAsync();
                                        TempData["SuccessedAlertMessage"] = "Update Successful";
                                    }
                                    else
                                    {
                                        return View("Error");
                                    }
                                }
                            }
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                    }
                }
                else
                {
                    if ((await authorizationService.AuthorizeAsync(User, "PageNamesInModulesUpdate")).Succeeded)
                    {
                        if (await PageExists(PageName: model.PageName, PageNamesInModuleId: null))
                        {
                            ModelState.AddModelError("PageName", "Page Name already exist");
                            return View(model);
                        }
                        else
                        {
                            pg.PageName = model.PageName;
                            pg.ModuleId = model.ModuleId;
                            pg.CreatedDate = CreatedDate;
                            await _context.PageNamesInModules.AddAsync(pg);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";
                        }
                    }
                    else
                    {
                        return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                    }
                }
                return RedirectToAction(nameof(PageNamesInModules));
            }
            else
            {
                ModelState.AddModelError("PageNamesInModules.PageName", "Page Name can't create!");

                return View(model);

            }
        }

        [HttpPost]
        [Authorize(Policy = "PageNamesInModulesDelete")]
        public async Task<IActionResult> DeletePageNamesInModules(int? PageNamesInModulesId)
        {
            ModelState.AddModelError("PageNamesInModules.PageName", "Page Name already exist");
            PageNamesInModulesViewModel model = new PageNamesInModulesViewModel();
            var totalpg = _context.PageNamesInModules.Include(x => x.Module).OrderBy(a => a.PageName).ToList();
            model.Modules = await _context.Modules.OrderBy(x => x.ModuleName).ToListAsync();
            model.PageNamesInModules = totalpg;
            var deletepg = await _context.PageNamesInModules.FindAsync(PageNamesInModulesId);
            try
            {

                if (deletepg != null)
                {
                    _context.PageNamesInModules.Remove(deletepg);
                    await _context.SaveChangesAsync();
                    TempData["SuccessedAlertMessage"] = "Delete Successful";
                }
                else
                {
                    //return View(model);
                    return View("Error");
                }
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Page Name ({deletepg.PageName}) can't delete!.These table are use in another Table");
                return View("PageNameAction", model);
            }
            return RedirectToAction(nameof(PageNamesInModules));

            //return View();
        }

        private async Task<bool> PageExists(string PageName, int? PageNamesInModuleId)
        {
            var pg = new PageNamesInModule();
            if (PageNamesInModuleId == null || PageNamesInModuleId.Value == 0)
            {
                pg = await _context.PageNamesInModules
                    .Where(x => x.PageName == PageName).FirstOrDefaultAsync();
            }
            else
            {
                pg = await _context.PageNamesInModules
                    .Where(x => x.PageNamesInModuleId != PageNamesInModuleId && x.PageName == PageName).FirstOrDefaultAsync();
            }
            if (pg == null)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        //[HttpGet]
        //public async Task<IActionResult> PageNameAction(int pageNameInModuleId, string btnActionName)
        //{
        //    ViewData["ModuleId"] = new SelectList(_context.Modules, "ModuleId", "ModuleName");
        //    var totalModules = await _context.PageNamesInModules.OrderBy(x=>x.PageName).ToListAsync();

        //    if (pageNameInModuleId == 0)
        //    {
        //        PageNamesInModulesViewModel pageVM = new PageNamesInModulesViewModel()
        //        {
        //            PageNamesInModules = totalModules,
        //            PageNamesInModule = new PageNamesInModule(),
        //            BtnActionName = "Create",
        //        };
        //        return View(pageVM);
        //    }
        //    else
        //    {
        //        List<PageNamesInModule> lstPages = new List<PageNamesInModule>();
        //        lstPages = await _context.PageNamesInModules.Where(a => a.PageNamesInModuleId == pageNameInModuleId).ToListAsync();
        //        PageNamesInModulesViewModel pageVM = new PageNamesInModulesViewModel()
        //        {
        //            PageNamesInModules = lstPages,
        //            PageNamesInModule = lstPages.FirstOrDefault(),
        //            BtnActionName = btnActionName,
        //        };
        //        return View(pageVM);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> PageNameAction(PageNamesInModulesViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        ViewData["ModuleId"] = new SelectList(_context.Modules, "ModuleId", "ModuleName");
        //        var totalModules = await _context.PageNamesInModules.ToListAsync();
        //        var PageExist = await _context.PageNamesInModules.Where(x => x.PageName == model.PageNamesInModule.PageName).FirstOrDefaultAsync();
        //        if (PageExist == null && model.BtnActionName != "Delete")
        //        {
        //            if (model.BtnActionName == "Edit")
        //            {
        //                try
        //                {
        //                    var dati = _context.PageNamesInModules
        //                   .Where(p => p.PageNamesInModuleId == model.PageNamesInModule.PageNamesInModuleId).Single();
        //                    dati.PageNamesInModuleId = model.PageNamesInModule.PageNamesInModuleId;
        //                    dati.PageName = model.PageNamesInModule.PageName;
        //                    await _context.SaveChangesAsync();
        //                    return RedirectToAction(nameof(PageNameAction));
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    if (!PageNamesInModuleExists(model.PageNamesInModule.PageNamesInModuleId))
        //                    {
        //                        return NotFound();
        //                    }
        //                    else
        //                    {
        //                        throw;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                await _context.PageNamesInModules.AddAsync(model.PageNamesInModule);
        //                await _context.SaveChangesAsync();
        //                return RedirectToAction(nameof(PageNameAction));
        //            }
        //        }
        //        else
        //        {
        //            if (model.BtnActionName == "Delete")
        //            {
        //                PageNamesInModule pageNamesInModule = await _context.PageNamesInModules
        //                    .FindAsync(model.PageNamesInModule.PageNamesInModuleId);
        //                _context.PageNamesInModules.Remove(pageNamesInModule);
        //                await _context.SaveChangesAsync();
        //                return RedirectToAction(nameof(PageNameAction));
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    ModelState.AddModelError("PageNamesInModule.PageName", "Page Name already exist");
        //                    return View(model);
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    if (!PageNamesInModuleExists(model.PageNamesInModule.PageNamesInModuleId))
        //                    {
        //                        return NotFound();
        //                    }
        //                    else
        //                    {
        //                        throw;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return View(model);
        //    }
        //}


        //[HttpPost]
        //private bool PageExists(string pageName, int moduleId)
        //{
        //    return _context.PageNamesInModules.Any(e => e.PageName == pageName && e.ModuleId == moduleId);
        //}


        //private bool PageNamesInModuleExists(int id)
        //{
        //    return _context.PageNamesInModules.Any(e => e.PageNamesInModuleId == id);
        //}
    }
}
