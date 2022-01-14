using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models.Administration;
using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Authorization;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class OnSetController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        [BindProperty]
        public OnSetViewModel OnSetViewModel { get; set; }
        public OnSetController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            OnSetViewModel = new OnSetViewModel()
            {
                OnSet = new OnSet(),
                OnSetList = new List<OnSet>(),
            };

        }

        // GET: Administration/Gender
        [Authorize(Policy = "OnsetView")]

        [HttpGet]
        public IActionResult OnSet()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            OnSetViewModel model = new OnSetViewModel();
            var total = _context.OnSets.OrderBy(a => a.OnSetName).ToList();
            model.OnSetList = total;
                return View(model);
          
        }
        [Authorize(Policy = "OnsetView")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnSet(OnSetViewModel model,string btnSubmit,int toDeleteId)

        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            btnSubmit = btnSubmit.ToLower().Trim();
            var total = await _context.OnSets.OrderBy(a => a.OnSetName).ToListAsync();
            model.OnSetList = total;
            OnSet onSet = new OnSet();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "OnSetDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var delete= await _context.OnSets.FindAsync(toDeleteId);
                try
                {
                    if (delete != null)
                    {
                        _context.OnSets.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(OnSet));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Onset Name ({delete.OnSetName}) can't delete!.These table are use in another Table");
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
                            if (model.OnSetId == null || model.OnSetId.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await Exist(onsetName: model.OnSetName, onsetId:model.OnSetId))
                                {
                                    ModelState.AddModelError("OnSetName", "OnSetName   already exist");
                                    return View(model);

                                }
                                else
                                {
                                    if (!(await authorizationService.AuthorizeAsync(User, "OnsetUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }
                                    var updated = await _context.OnSets.FindAsync(model.OnSetId.Value);
                                    updated.OnSetName = model.OnSetName;
                                    updated.UpdatedDate = UpdatedDate;
                                    await _context.SaveChangesAsync();
                                    TempData["SuccessedAlertMessage"] = "Update Successful";

                                    return RedirectToAction(nameof(OnSet));
                                }
                            }
                        }
                        catch
                        {
                            throw;
                        }

                    }
                    else
                    {
                        if (await Exist(onsetName: model.OnSetName, onsetId: null))
                        {
                            ModelState.AddModelError("GenderName", "Gender Name already exist");
                            return View(model);
                        }
                        else
                        {
                            if (!(await authorizationService.AuthorizeAsync(User, "OnsetCreate")).Succeeded)
                            {
                                return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                            }
                            onSet.OnSetName = model.OnSetName;
                            onSet.CreatedDate = CreatedDate;
                            await _context.OnSets.AddAsync(onSet);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";

                            return RedirectToAction(nameof(OnSet));
                        }
                       
                    }


                }
                else
                {
                    ModelState.AddModelError("OnSetName", "OnSetName can't create!");

                    return View(model);
                }
            }
         
        }
        private async Task<bool> Exist(string onsetName, int? onsetId)
        {
            var onset = new OnSet();
            if (onsetId == null || onsetId.Value == 0)
            {
                onset = await _context.OnSets
                    .Where(x => x.OnSetName == onsetName).FirstOrDefaultAsync();
            }
            else
            {
                onset = await _context.OnSets
                    .Where(x => x.OnSetId != onsetId && x.OnSetName == onsetName).FirstOrDefaultAsync();
            }
            if (onset == null)
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
