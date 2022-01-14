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
    public class OnSetTypeController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        [BindProperty]
        public OnSetTypeViewModel OnSetTypeViewModel { get; set; }
        public OnSetTypeController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            OnSetTypeViewModel = new OnSetTypeViewModel()
            {
                OnSetType = new OnSetType(),
                OnSetTypeList = new List<OnSetType>(),
            };

        }

        // GET: Administration/Gender
        [Authorize(Policy = "OnsetTypeView")]

        [HttpGet]
        public IActionResult OnSetType()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            OnSetTypeViewModel model = new OnSetTypeViewModel();
            var total = _context.OnSetTypes.OrderBy(a => a.OnSetTypeName).ToList();
            model.OnSetTypeList = total;
             return View(model);
         

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnSetType(OnSetTypeViewModel model,string btnSubmit,int toDeleteId)

        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            btnSubmit = btnSubmit.ToLower().Trim();
            OnSetType onSetType = new OnSetType();
            var total = await _context.OnSetTypes.OrderBy(a => a.OnSetTypeName).ToListAsync();
            model.OnSetTypeList = total;
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "OnSetTypeDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var delete = await _context.OnSetTypes.FindAsync(toDeleteId);
                try
                {
                    if (delete != null)
                    {
                        _context.OnSetTypes.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(OnSetType));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"OnsetType Name ({delete.OnSetTypeName}) can't delete!.These table are use in another Table");
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
                            if (model.OnSetTypeId == null || model.OnSetTypeId.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await Exist(onsetTypeName: model.OnSetTypeName, onsetTypeId: model.OnSetTypeId))
                                {
                                    ModelState.AddModelError("OnSetTypeName", "OnSetTypeName   already exist");
                                    return View(model);

                                }
                                else
                                {
                                    if (!(await authorizationService.AuthorizeAsync(User, "OnsetTypeUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }
                                    var updated = await _context.OnSetTypes.FindAsync(model.OnSetTypeId.Value);
                                    updated.OnSetTypeName = model.OnSetTypeName;
                                    updated.UpdatedDate = UpdatedDate;
                                    await _context.SaveChangesAsync();
                                    TempData["SuccessedAlertMessage"] = "Update Successful";

                                    return RedirectToAction(nameof(OnSetType));
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
                        if (await Exist(onsetTypeName: model.OnSetTypeName, onsetTypeId: null))
                        {
                            ModelState.AddModelError("OnSetTypeName", "OnSetTypeName  already exist");
                            return View(model);
                        }
                        else
                        {
                            if (!(await authorizationService.AuthorizeAsync(User, "OnsetTypeCreate")).Succeeded)
                            {
                                return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                            }

                            onSetType.OnSetTypeName = model.OnSetTypeName;
                            onSetType.CreatedDate = CreatedDate;
                            await _context.OnSetTypes.AddAsync(onSetType);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";

                            return RedirectToAction(nameof(OnSetType));
                        }
                      
                    }


                }
                else
                {
                    ModelState.AddModelError("OnSetTypeName", "OnSetTypeName can't create!");

                    return View(model);
                }
            }
          
        }
        private async Task<bool> Exist(string onsetTypeName, int? onsetTypeId)
        {
            var onsetType = new OnSetType();
            if (onsetTypeId == null || onsetTypeId.Value == 0)
            {
                onsetType = await _context.OnSetTypes
                    .Where(x => x.OnSetTypeName == onsetTypeName).FirstOrDefaultAsync();
            }
            else
            {
                onsetType = await _context.OnSetTypes
                    .Where(x => x.OnSetTypeId != onsetTypeId && x.OnSetTypeName == onsetTypeName).FirstOrDefaultAsync();
            }
            if (onsetType == null)
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
