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
//using Stripe.Issuing;
using Microsoft.AspNetCore.Identity;
using HMS.Extensions;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize]
    public class UnitOfMeasurementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService authorizationService;
        public UnitOfMeasurementViewModel UnitOfMeasurementVM { get; set; }

        public UnitOfMeasurementController(ApplicationDbContext context,
                                           IAuthorizationService authorizationService)
        {
            _context = context;
            this.authorizationService = authorizationService;
            UnitOfMeasurementVM = new UnitOfMeasurementViewModel()
            {
                UnitOfMeasurements = new List<UnitOfMeasurement>(),
            };
        }

        [HttpGet]
        [Authorize(Policy = "UnitOfMeasurementView")]
        public async Task<IActionResult> UnitOfMeasurement()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            UnitOfMeasurementViewModel model = new UnitOfMeasurementViewModel();
            model.UnitOfMeasurements = await _context.UnitOfMeasurements.OrderBy(x => x.Description).ToListAsync();
            return View(model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "UnitOfMeasurementView")]
        public async Task<IActionResult> UnitOfMeasurement(UnitOfMeasurementViewModel model, string btnSubmit,int toDeleteUOMId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            model.UnitOfMeasurements = await _context.UnitOfMeasurements.OrderBy(x => x.Description).ToListAsync();
            UnitOfMeasurement unit = new UnitOfMeasurement();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "UnitOfMeasurementDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var delete = await _context.UnitOfMeasurements.FindAsync(toDeleteUOMId);
                try
                {
                    if (delete != null)
                    {
                        _context.UnitOfMeasurements.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(UnitOfMeasurement));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Unit Of Measurement ({delete.Description}) can't delete!.These table are use in another Table");
                    return View(model);
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    //var NameExist = await _context.Townships.Where(x => x.Tsp_name == model.Township.Tsp_name).FirstOrDefaultAsync();
                    if (btnSubmit == "update")
                    {
                        if ((await authorizationService.AuthorizeAsync(User, "UnitOfMeasurementUpdate")).Succeeded)
                        {
                            try
                            {
                                if (model.UnitOfMeasurementId == null || model.UnitOfMeasurementId.Value == 0)
                                {
                                    return View("Error");
                                }
                                else
                                {
                                    //if (await UomExists(Description: model.Description, Code: model.Code, UnitOfMeasurementId: model.UnitOfMeasurementId))
                                    //{
                                    //    ModelState.AddModelError("Description", "Description already exist");
                                    //    return View(model);

                                    //}
                                    //else
                                    //{
                                        var updated = await _context.UnitOfMeasurements.FindAsync(model.UnitOfMeasurementId.Value);
                                        if (updated != null)
                                        {
                                            updated.Description = model.Description;
                                            updated.Code = model.Code;
                                            updated.UpdatedDate = UpdatedDate;
                                            _context.UnitOfMeasurements.Update(updated);
                                            await _context.SaveChangesAsync();
                                            TempData["SuccessedAlertMessage"] = "Update Successful";
                                        }
                                        else
                                        {
                                            return View("Error");
                                        }
                                    //}
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
                        if ((await authorizationService.AuthorizeAsync(User, "UnitOfMeasurementUpdate")).Succeeded)
                        {
                            //if (await UomExists(Description: model.Description, Code: model.Code, UnitOfMeasurementId: model.UnitOfMeasurementId))
                            //{
                            //    ModelState.AddModelError("Description", "Description already exist");
                            //    return View(model);
                            //}
                            //else
                            //{
                                unit.Description = model.Description;
                                unit.Code = model.Code;
                                unit.CreatedDate = CreatedDate;
                                await _context.UnitOfMeasurements.AddAsync(unit);
                                await _context.SaveChangesAsync();
                                TempData["SuccessedAlertMessage"] = "Save Successful";
                            //}
                        }
                        else
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                        }
                    }
                    return RedirectToAction(nameof(UnitOfMeasurement));
                }
                else
                {
                    ModelState.AddModelError("UnitOfMeasurement.Description", "Unit Of Measurement can't create!");

                    return View(model);

                }
            }
        }

        //[HttpPost]
        //[Authorize(Policy = "UnitOfMeasurementDelete")]
        //public async Task<IActionResult> DeleteUnitOfMeasurement(int? UnitOfMeasurementId)
        //{
        //    ModelState.AddModelError("UnitOfMeasurement.Description", "Unit Of Measurement already exist");
        //    UnitOfMeasurementViewModel model = new UnitOfMeasurementViewModel();
        //    model.UnitOfMeasurements = await _context.UnitOfMeasurements.OrderBy(x => x.Description).ToListAsync();
        //    var deleteuom = await _context.UnitOfMeasurements.FindAsync(UnitOfMeasurementId);
        //    try
        //    {

        //        if (deleteuom != null)
        //        {
        //            _context.UnitOfMeasurements.Remove(deleteuom);
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
        //        ModelState.AddModelError("", $"Unit Of Measurement ({deleteuom.Description}) can't delete!.These table are use in another Table");
        //        return View("UnitOfMeasurement", model);
        //    }
        //    return RedirectToAction(nameof(UnitOfMeasurement));

        //    //return View();
        //}

        //private async Task<bool> UomExists(string Description,string Code,int? UnitOfMeasurementId)
        //{
        //    var unit = new UnitOfMeasurement();
        //    if (UnitOfMeasurementId != null)
        //    {
        //        unit = await _context.UnitOfMeasurements
        //            .Where(x=> x.Description == Description && x.Code==Code && x.UnitOfMeasurementId!=UnitOfMeasurementId).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        unit = await _context.UnitOfMeasurements
        //           .Where(x => x.Description == Description || x.Code == Code).FirstOrDefaultAsync();
        //    }
        //    if (unit == null)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }

        //}
        //[AllowAnonymous]
        //[AcceptVerbs("Get", "Post")]
        //public async Task<IActionResult> DescriptionIsInUse(string Description, int? UnitOfMeasurementId)
        //{
        //    UnitOfMeasurement uom = new UnitOfMeasurement();
        //    if (UnitOfMeasurementId == null || UnitOfMeasurementId.Value == 0)
        //    {
        //        uom = await _context.UnitOfMeasurements.Where(x => x.Description.Replace(" ", string.Empty).ToLower().Trim()
        //        == Description.StringCompareFormat() || x.Description.Replace(" ", string.Empty).ToLower().Trim()
        //        == Description.StringCompareFormat()).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        uom = await _context.UnitOfMeasurements.Where(x => x.UnitOfMeasurementId != UnitOfMeasurementId.Value && (x.Description.Replace(" ", string.Empty).ToLower().Trim()
        //        == Description.StringCompareFormat() || x.Code.Replace(" ", string.Empty).ToLower().Trim()
        //        == Description.StringCompareFormat())).FirstOrDefaultAsync();
        //    }

        //    if (uom == null)
        //    {
        //        return Json(true);
        //    }
        //    else
        //    {
        //        return Json($"Description {Description} is already exist in current table");
        //    }
        //}

        //[AllowAnonymous]
        //[AcceptVerbs("Get", "Post")]
        //public async Task<IActionResult> CodeIsInUse(string Code, int? UnitOfMeasurementId)
        //{
        //    UnitOfMeasurement uom = new UnitOfMeasurement();
        //    if (UnitOfMeasurementId == null || UnitOfMeasurementId.Value == 0)
        //    {
        //        uom = await _context.UnitOfMeasurements.Where(x => x.Description.Replace(" ", string.Empty).ToLower().Trim()
        //        == Code.StringCompareFormat() || x.Code.Replace(" ", string.Empty).ToLower().Trim()
        //        == Code.StringCompareFormat()).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        uom = await _context.UnitOfMeasurements.Where(x => x.UnitOfMeasurementId != UnitOfMeasurementId.Value && (x.Description.Replace(" ", string.Empty).ToLower().Trim()
        //        == Code.StringCompareFormat() || x.Code.Replace(" ", string.Empty).ToLower().Trim()
        //        == Code.StringCompareFormat())).FirstOrDefaultAsync();
        //    }
        //    if (uom == null)
        //    {
        //        return Json(true);
        //    }
        //    else
        //    {
        //        return Json($"Code {Code} is already exist in currency table");
        //    }


        //}
        //[HttpGet]
        //[Authorize(Policy = "UnitOfMeasurementView")]
        //public async Task<IActionResult> UnitOfMeasurement(int UnitOfMeasurementId, string btnSubmit)
        //{
        //    ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
        //    var totalUOM = _context.UnitOfMeasurements.OrderBy(x => x.Description).ToList();
        //    var currentUOM = totalUOM.Where(a => a.UnitOfMeasurementId == UnitOfMeasurementId).FirstOrDefault();
        //    if (UnitOfMeasurementId == 0)
        //    {
        //        UnitOfMeasurementViewModel uomVM = new UnitOfMeasurementViewModel()
        //        {
        //            UnitOfMeasurements = totalUOM,
        //        };

        //        return View(uomVM);
        //    }
        //    else
        //    {
        //        if (!((await authorizationService.AuthorizeAsync(User, "UnitOfMeasurementUpdate")).Succeeded ||
        //          (await authorizationService.AuthorizeAsync(User, "UnitOfMeasurementDelete")).Succeeded))
        //        {
        //            return RedirectToAction("AccessDenied", "UserAccount", "Administration");

        //        }
        //        List<UnitOfMeasurement> lstUOM = new List<UnitOfMeasurement>();
        //        lstUOM = await _context.UnitOfMeasurements.Where(a => a.UnitOfMeasurementId == UnitOfMeasurementId).ToListAsync();
        //        UnitOfMeasurementViewModel uomVm = new UnitOfMeasurementViewModel()
        //        {
        //            UnitOfMeasurements = totalUOM,
        //            UnitOfMeasurement = lstUOM.FirstOrDefault(),
        //            BtnActionName = btnActionName,
        //        };
        //        return View(uomVm);
        //    }

        //}

        //[Authorize(Policy = "UnitOfMeasurementView")]
        //[HttpPost, ActionName("UnitOfMeasurement")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UnitOfMeasurement(UnitOfMeasurementViewModel model, DateTime cDate, DateTime uDate, string btnSubmit)
        //{
        //    var totalUOM = await _context.UnitOfMeasurements.OrderBy(a => a.Code).ToListAsync();
        //    model.UnitOfMeasurements = totalUOM;
        //    var codeExist = await _context.UnitOfMeasurements.Where(x => x.Code == model.UnitOfMeasurement.Code).FirstOrDefaultAsync();
        //    var descriptionExist = await _context.UnitOfMeasurements.Where(x => x.Description == model.UnitOfMeasurement.Description).FirstOrDefaultAsync();
        //    if (ModelState.IsValid)
        //    {
        //        cDate = DateTime.UtcNow.AddMinutes(390);
        //        uDate = DateTime.UtcNow.AddMinutes(390);
        //        model.UnitOfMeasurement.CreatedDate = cDate;
        //        model.UnitOfMeasurement.UpdatedDate = uDate;
        //        if (model.BtnActionName != "Delete")
        //        {
        //            if (btnSubmit == "Update")
        //            {
        //                if (!(await authorizationService.AuthorizeAsync(User, "UnitOfMeasurementId")).Succeeded)
        //                {
        //                    return RedirectToAction("AccessDenied", "UserAccount", "Administration");
        //                }
        //                try
        //                {
        //                    UnitOfMeasurement unitOfMeasurement = await _context.UnitOfMeasurements.FindAsync(model.UnitOfMeasurementId);
        //                    if (unitOfMeasurement != null)
        //                    {
        //                        unitOfMeasurement.Description = model.Description;
        //                        _context.UnitOfMeasurements.Update(unitOfMeasurement);
        //                        await _context.SaveChangesAsync();
        //                        TempData["SuccessedAlertMessage"] = "Update Successful";

        //                    }
        //                    else
        //                    {
        //                        return View("Error");
        //                    }
        //                }
        //                catch
        //                {
        //                    return View("Error");
        //                }

        //                return RedirectToAction(nameof(UnitOfMeasurement));
        //            }
        //            //if (!(await authorizationService.AuthorizeAsync(User, "UnitOfMeasurementUpdate")).Succeeded)
        //            //{
        //            //    return RedirectToAction("AccessDenied", "UserAccount", "Administration");

        //            //}
        //            //try
        //            //{
        //            //    var currentData = _context.UnitOfMeasurements.Where(p => p.UnitOfMeasurementId == model.UnitOfMeasurementId).Single();
        //            //    currentData.Description = model.UnitOfMeasurement.Description;
        //            //    currentData.Code = model.UnitOfMeasurement.Code;
        //            //    currentData.UpdatedDate = uDate;
        //            //    await _context.SaveChangesAsync();
        //            //    TempData["SuccessedAlertMessage"] = "Update Successful";
        //            //    return RedirectToAction(nameof(UnitOfMeasurement));
        //            //}
        //            //catch (DbUpdateConcurrencyException)
        //            //{
        //            //    if (codeExist != null)
        //            //    {
        //            //        return NotFound();
        //            //    }
        //            //    else
        //            //    {
        //            //        throw;
        //            //    }
        //            //}

        //            else
        //            {
        //                if (descriptionExist != null)
        //                {
        //                    ModelState.AddModelError("UnitOfMeasurement.Description", "Description already exist");
        //                    return View(model);
        //                }
        //                if (codeExist != null)
        //                {
        //                    ModelState.AddModelError("UnitOfMeasurement.Code", "Code already exist");
        //                    return View(model);
        //                }
        //                if (!(await authorizationService.AuthorizeAsync(User, "UnitOfMeasurementCreate")).Succeeded)
        //                {
        //                    return RedirectToAction("AccessDenied", "UserAccount", "Administration");

        //                }
        //                model.UnitOfMeasurement.CreatedDate = cDate;
        //                model.UnitOfMeasurement.UpdatedDate = uDate;
        //                await _context.UnitOfMeasurements.AddAsync(model.UnitOfMeasurement);
        //                await _context.SaveChangesAsync();
        //                TempData["SuccessedAlertMessage"] = "Save Successful";
        //                return RedirectToAction(nameof(UnitOfMeasurement));
        //            }
        //        }
        //        else
        //        {
        //            if (model.BtnActionName == "Delete")
        //            {
        //                if (!(await authorizationService.AuthorizeAsync(User, "UnitOfMeasurementDelete")).Succeeded)
        //                {
        //                    return RedirectToAction("AccessDenied", "UserAccount", "Administration");
        //                }
        //                UnitOfMeasurement uom = await _context.UnitOfMeasurements
        //                .FindAsync(model.UnitOfMeasurement.UnitOfMeasurementId);
        //                _context.UnitOfMeasurements.Remove(uom);
        //                await _context.SaveChangesAsync();
        //                TempData["SuccessedAlertMessage"] = "Delete Successful";
        //                return RedirectToAction(nameof(UnitOfMeasurement));
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    ModelState.AddModelError("UnitOfMeasurement.Code", "Code already exist");
        //                    return View(model);
        //                }
        //                catch (DbUpdateException ex)
        //                {
        //                    return View(model);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {

        //        var NameExist = await _context.UnitOfMeasurements.Where(x => x.Description == model.UnitOfMeasurement.Description).FirstOrDefaultAsync();
        //        if (NameExist != null)
        //        {
        //            ModelState.AddModelError("UnitOfMeasurement.Description", "Description already exist");

        //            return View(model);
        //        }
        //        else
        //        {
        //            model.UnitOfMeasurement.CreatedDate = cDate;
        //            model.UnitOfMeasurement.UpdatedDate = uDate;
        //            await _context.UnitOfMeasurements.AddAsync(model.UnitOfMeasurement);
        //            await _context.SaveChangesAsync();
        //            ModelState.AddModelError("UnitOfMeasurement.Code", "Code can't create!");
        //            return RedirectToAction(nameof(UnitOfMeasurement));
        //        }
        //    }
        //}
        //[HttpPost]
        //[Authorize(Policy = "UnitDelete")]
        //public async Task<IActionResult> DeleteUnitOfMeasurement(int? id)
        //{
        //    ModelState.AddModelError("Unit.UnitName", "Unit Name already exist");
        //    UnitOfMeasurementViewModel model = new UnitOfMeasurementViewModel();
        //    model.UnitOfMeasurements = await _context.UnitOfMeasurements.OrderBy(x => x.Description).ToListAsync();
        //    var deleteUnitOfMeasurement = await _context.UnitOfMeasurements.FindAsync(id);
        //    try
        //    {

        //        if (deleteUnitOfMeasurement != null)
        //        {
        //            _context.UnitOfMeasurements.Remove(deleteUnitOfMeasurement);
        //            await _context.SaveChangesAsync();
        //            TempData["SuccessedAlertMessage"] = "Delete Successful";
        //        }
        //        else
        //        {
        //            return View("Error");
        //        }
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        ModelState.AddModelError("", $"Description ({deleteUnitOfMeasurement.Description}) can't delete!.These table are use in another Table");
        //        return View("UnitOfMeasurement", model);
        //    }
        //    return RedirectToAction(nameof(UnitOfMeasurement));

        //    //return View();
        //}
    }
}

