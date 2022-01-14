using HMS.Data;
using HMS.Models.Administration;
using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]

    public class PrefixController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        public PrefixViewModel PrefixViewModel  { get; set; }
        public PrefixController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            PrefixViewModel = new PrefixViewModel()
            {
                GenderList = _context.Genders.ToList(),
            
                PrefixList = new List<Prefix>(),
            };

        }


        [HttpGet]
        [Authorize(Policy = "PrefixView")]
        public async Task<IActionResult> Prefix()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            PrefixViewModel model = new PrefixViewModel();
            var totalPrefix = _context.Prefixes.Include(x => x.Gender).OrderBy(a => a.Gender.GenderName).ToList();
            model.GenderList = await _context.Genders.OrderBy(x => x.GenderName).ToListAsync();
            model.PrefixList = totalPrefix;
          
            return View(model);
         

        }

        [HttpPost]
        [Authorize(Policy = "PrefixView")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Prefix(PrefixViewModel model, string btnSubmit,int ToDeleteId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalPrefix = _context.Prefixes.Include(x => x.Gender).OrderBy(a => a.PrefixName).ToList();
            var total = await _context.Prefixes.OrderBy(a => a.PrefixName).ToListAsync();
            model.GenderList = await _context.Genders.OrderBy(x => x.GenderName).ToListAsync();
            model.PrefixList = totalPrefix;
            Prefix prefix  = new Prefix();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "PrefixDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var deletePrefix = await _context.Prefixes.FindAsync(ToDeleteId);
                try
                {
                    if (deletePrefix != null)
                    {
                        _context.Prefixes.Remove(deletePrefix);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(Prefix));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Prefix Name ({deletePrefix.PrefixName}) can't delete!.These table are use in another Table");
                    return View(model);
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (btnSubmit == "update")
                    {

                        try
                        {
                            if (model.Prefix_Id == null || model.Prefix_Id.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await PrefixExist(prefixName: model.PrefixName, prefixId: model.Prefix_Id))
                                {
                                    ModelState.AddModelError("PrefixName", "Prefix Name already exist");
                                    return View(model);

                                }
                                else
                                {


                                    if (!(await authorizationService.AuthorizeAsync(User, "PrefixUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }

                                    var updatedPrefix = await _context.Prefixes.FindAsync(model.Prefix_Id.Value);
                                    if (updatedPrefix != null)
                                    {
                                        updatedPrefix.PrefixName = model.PrefixName;
                                        updatedPrefix.GenderId = model.GenderId;
                                        updatedPrefix.UpdatedDate = UpdatedDate;
                                        _context.Prefixes.Update(updatedPrefix);
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
                        if ((await authorizationService.AuthorizeAsync(User, "PrefixCreate")).Succeeded)
                        {
                            if (await PrefixExist(prefixName: model.PrefixName, prefixId: null))
                            {
                                ModelState.AddModelError("PrefixName", "PrefixName  already exist");
                                return View(model);
                            }
                            else
                            {

                                prefix.PrefixName = model.PrefixName;
                                prefix.GenderId = model.GenderId;
                                prefix.CreatedDate = CreatedDate;
                                await _context.Prefixes.AddAsync(prefix);
                                await _context.SaveChangesAsync();
                                TempData["SuccessedAlertMessage"] = "Save Successful";


                            }
                        }
                        else
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                        }

                    }
                    return RedirectToAction(nameof(Prefix));

                }
                else
                {

                    ModelState.AddModelError("PrefixName", "Prefix Name can't create!");

                    return View(model);

                }
            }
          



        }
        //[HttpPost]
        //[Authorize(Policy = "PrefixDelete")]
        //public async Task<IActionResult> DeletePrefix(int? prefixId)
        //{
        //    ModelState.AddModelError("Prefix.PrefixName", "Prefix Name already exist");
        //    PrefixViewModel model = new PrefixViewModel();
        //    var totalPrefixs = _context.Prefixes.Include(x => x.Gender).OrderBy(a => a.PrefixName).ToList();
        //    model.GenderList = await _context.Genders.OrderBy(x => x.GenderName).ToListAsync();
        //    model.PrefixList = totalPrefixs;
        //    var deletePrefix = await _context.Prefixes.FindAsync(prefixId);
        //    try
        //    {

        //        if (deletePrefix != null)
        //        {
        //            _context.Prefixes.Remove(deletePrefix);
        //            await _context.SaveChangesAsync();
        //            TempData["SuccessedAlertMessage"] = "Delete Successful";
        //        }
        //        else
        //        {
        //            //return View(model);
        //            return View("Error");
        //        }
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        ModelState.AddModelError("", $"Prefix Name ({deletePrefix.PrefixName}) can't delete!.These table are use in another Table");
        //        return View("Prefix", model);
        //    }
        //    return RedirectToAction(nameof(Prefix));

        //    //return View();
        //}

        private async Task<bool> PrefixExist(string prefixName, int? prefixId)
        {
            var prefix = new Prefix();
            if (prefixId == null || prefixId.Value == 0)
            {
                prefix = await _context.Prefixes
                    .Where(x => x.PrefixName == prefixName).FirstOrDefaultAsync();
            }
            else
            {
                prefix = await _context.Prefixes
                    .Where(x => x.Prefix_Id != prefixId && x.PrefixName == prefixName).FirstOrDefaultAsync();
            }
            if (prefix == null)
            {
                return false;
            }
            else
            {
                return true;
            }
         
        }

    }
}