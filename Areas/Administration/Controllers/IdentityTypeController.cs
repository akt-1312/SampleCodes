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
    public class IdentityTypeController : Controller
    {
        private readonly IAuthorizationService authorizationService;
        private readonly ApplicationDbContext _context;
        public IdentityTypeViewModel IdentityTypeViewModel { get; set; }
      
        public IdentityTypeController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            IdentityTypeViewModel = new IdentityTypeViewModel()
            {
                IdentityType = new IdentityType(),
                IdentityTypeList = new List<IdentityType>(),
            };
        }

        // GET: Administration/IdentityType
        [Authorize(Policy = "IdentityTypeView")]

        [HttpGet]
        public IActionResult IdentityType()
        {

            

            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            IdentityTypeViewModel model = new IdentityTypeViewModel();
            var total = _context.IdentityTypes.OrderBy(a => a.IdentityTypeName).ToList();
            model.IdentityTypeList = total;
            return View(model);
         

        }
        [Authorize(Policy = "IdentityTypeView")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IdentityType(IdentityTypeViewModel model,string btnSubmit,int toDeleteId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            btnSubmit = btnSubmit.ToLower().Trim();
            var total = await _context.IdentityTypes.OrderBy(a => a.IdentityTypeName).ToListAsync();
            model.IdentityTypeList = total;
            IdentityType identityType = new IdentityType();

            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "IdentityTypeDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var deleteIdentity = await _context.IdentityTypes.FindAsync(toDeleteId);
                try
                {
                    if (deleteIdentity != null)
                    {
                        _context.IdentityTypes.Remove(deleteIdentity);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(IdentityType));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"IdentityType Name ({deleteIdentity.IdentityTypeName}) can't delete!.These table are use in another Table");
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
                            if (model.IdentityTypeId == null || model.IdentityTypeId.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await IdentityTypeExist(identityName: model.IdentityTypeName, identityId: model.IdentityTypeId))
                                {
                                    ModelState.AddModelError("IdentityTypeName", "IdentityType Name   already exist");
                                    return View(model);

                                }
                                else
                                {
                                    if (!(await authorizationService.AuthorizeAsync(User, "IdentityTypeUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }
                                    var updated = await _context.IdentityTypes.FindAsync(model.IdentityTypeId.Value);

                                    updated.IdentityTypeName = model.IdentityTypeName;
                                    updated.UpdatedDate = UpdatedDate;
                                    await _context.SaveChangesAsync();
                                    TempData["SuccessedAlertMessage"] = "Update Successful";

                                    return RedirectToAction(nameof(IdentityType));
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
                        if (await IdentityTypeExist(identityName: model.IdentityTypeName, identityId: null))
                        {
                            ModelState.AddModelError("IdentityTypeName", "IdentityType Name  already exist");
                            return View(model);
                        }
                        else
                        {
                            if (!(await authorizationService.AuthorizeAsync(User, "IdentityTypeCreate")).Succeeded)
                            {
                                return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                            }
                            identityType.IdentityTypeName = model.IdentityTypeName;
                            identityType.CreatedDate = CreatedDate;
                            await _context.IdentityTypes.AddAsync(identityType);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";

                            return RedirectToAction(nameof(IdentityType));
                        }
                    }
                }


                else
                {

                    ModelState.AddModelError("IdentityTypeName", "IdentityTypeName can't create!");

                    return View(model);
                }
            }
          
        }


        private async Task<bool> IdentityTypeExist(string identityName, int? identityId)
        {
            var identity = new IdentityType();
            if (identityId == null || identityId.Value == 0)
            {
                identity = await _context.IdentityTypes
                    .Where(x => x.IdentityTypeName == identityName).FirstOrDefaultAsync();
            }
            else
            {
                identity = await _context.IdentityTypes
                    .Where(x => x.IdentityTypeId != identityId && x.IdentityTypeName == identityName).FirstOrDefaultAsync();
            }
            if (identity == null)
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
