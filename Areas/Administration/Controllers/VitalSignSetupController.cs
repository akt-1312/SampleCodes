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
using HMS.Extensions;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class VitalSignSetupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService authorizationService;

        public VitalSignSetUpViewModel VitalSignSetupVM { get; set; }
        public VitalSignSetupController(ApplicationDbContext context,
                                        IAuthorizationService authorizationService)
        {
            _context = context;
            this.authorizationService = authorizationService;
            VitalSignSetupVM = new VitalSignSetUpViewModel()
            {
                UnitOfMeasurements = _context.UnitOfMeasurements.ToList(),
                VitalSignSetups = new List<VitalSignSetup>(),
            };

        }
        [HttpGet]
        [Authorize(Policy = "VitalSignsSetupView")]
        public async Task<IActionResult> VitalSignSetup()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            VitalSignSetUpViewModel model = new VitalSignSetUpViewModel();
            var totalUnits = _context.VitalSignSetups.Include(x => x.UnitOfMeasurement).OrderBy(a => a.Description).ToList();
            model.UnitOfMeasurements = await _context.UnitOfMeasurements.OrderBy(x => x.Description).ToListAsync();
            model.VitalSignSetups = totalUnits;
            return View(model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "VitalSignsSetupView")]
        public async Task<IActionResult> VitalSignSetup(VitalSignSetUpViewModel model, string btnSubmit, int toDeleteVSId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalUnits = _context.VitalSignSetups.Include(x => x.UnitOfMeasurement).OrderBy(a => a.Description).ToList();
            var total = await _context.VitalSignSetups.OrderBy(a => a.Description).ToListAsync();
            model.UnitOfMeasurements = await _context.UnitOfMeasurements.OrderBy(x => x.Description).ToListAsync();
            model.VitalSignSetups = totalUnits;
            VitalSignSetup vs = new VitalSignSetup();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "VitalSignsSetupDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var delete = await _context.VitalSignSetups.FindAsync(toDeleteVSId);
                try
                {
                    if (delete != null)
                    {
                        _context.VitalSignSetups.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(VitalSignSetup));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Vital Sign ({delete.Description}) can't delete!.These table are use in another Table");
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
                        if ((await authorizationService.AuthorizeAsync(User, "VitalSignsSetupUpdate")).Succeeded)
                        {
                            try
                            {
                                if (model.VitalSignSetupId == null || model.VitalSignSetupId.Value == 0)
                                {
                                    return View("Error");
                                }
                                else
                                {
                                    var updated = await _context.VitalSignSetups.FindAsync(model.VitalSignSetupId.Value);
                                    if (updated != null)
                                    {
                                        updated.Description = model.Description;
                                        updated.UnitOfMeasurementId = model.UnitOfMeasurementId;
                                        updated.MinRange = model.MinRange;
                                        updated.MaxRange = model.MaxRange;
                                        updated.UpdatedDate = UpdatedDate;
                                        _context.VitalSignSetups.Update(updated);
                                        await _context.SaveChangesAsync();
                                        TempData["SuccessedAlertMessage"] = "Update Successful";
                                    }
                                    else
                                    {
                                        return View("Error");
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
                        if ((await authorizationService.AuthorizeAsync(User, "VitalSignsSetupUpdate")).Succeeded)
                        {
                            vs.Description = model.Description;
                            vs.UnitOfMeasurementId = model.UnitOfMeasurementId;
                            vs.MinRange = model.MinRange;
                            vs.MaxRange = model.MaxRange;
                            vs.CreatedDate = CreatedDate;
                            await _context.VitalSignSetups.AddAsync(vs);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";
                        }
                        else
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                        }
                    }
                    return RedirectToAction(nameof(VitalSignSetup));
                }
                else
                {
                    return View(model);
                }
            }
        }
        [AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> DescriptionIsInUse(string Description, int? VitalSignSetupId)
        {
            VitalSignSetup vs = new VitalSignSetup();
            if (VitalSignSetupId == null || VitalSignSetupId.Value == 0)
            {
                vs = await _context.VitalSignSetups.Where(x => x.Description.Replace(" ", string.Empty).ToLower().Trim()
                == Description.StringCompareFormat() || x.Description.Replace(" ", string.Empty).ToLower().Trim()
                == Description.StringCompareFormat()).FirstOrDefaultAsync();
            }
            else
            {
                vs = await _context.VitalSignSetups.Where(x => x.VitalSignSetupId != VitalSignSetupId.Value && (x.Description.Replace(" ", string.Empty).ToLower().Trim()
                == Description.StringCompareFormat() || x.UnitOfMeasurement.Code.Replace(" ", string.Empty).ToLower().Trim()
                == Description.StringCompareFormat())).FirstOrDefaultAsync();
            }

            if (vs == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Description {Description} is already exist in current table");
            }
        }

        [AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CodeIsInUse(int UnitOfMeasurementId, int? VitalSignSetupId)
        {
            VitalSignSetup vs = new VitalSignSetup();
            if (VitalSignSetupId == null || VitalSignSetupId.Value == 0)
            {
                vs = await _context.VitalSignSetups.Where(x => x.Description.Replace(" ", string.Empty).ToLower().Trim()
                == (UnitOfMeasurementId).ToString().StringCompareFormat() || x.UnitOfMeasurementId.ToString().Replace(" ", string.Empty).ToLower().Trim()
                == (UnitOfMeasurementId).ToString().StringCompareFormat()).FirstOrDefaultAsync();
            }
            else
            {
                vs = await _context.VitalSignSetups.Where(x => x.VitalSignSetupId != VitalSignSetupId.Value && (x.UnitOfMeasurementId.ToString().Replace(" ", string.Empty).ToLower().Trim()
                == (UnitOfMeasurementId).ToString().StringCompareFormat() || x.UnitOfMeasurementId.ToString().Replace(" ", string.Empty).ToLower().Trim()
                == (UnitOfMeasurementId).ToString().StringCompareFormat())).FirstOrDefaultAsync();
            }
            if (vs == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Code {UnitOfMeasurementId} is already exist in currency table");
            }


        }
        //[HttpPost]
        //[Authorize(Policy = "VitalSignsSetupDelete")]
        //public async Task<IActionResult> DeleteVitalSign(int? VitalSignSetUpId)
        //{
        //    ModelState.AddModelError("VitalSignSetUp.Desctiption", "Vital Sign Name already exist");
        //    VitalSignSetUpViewModel model = new VitalSignSetUpViewModel();
        //    var totalvs = _context.VitalSignSetups.Include(x => x.UnitOfMeasurement).OrderBy(a => a.Desctiption).ToList();
        //    model.UnitOfMeasurements = await _context.UnitOfMeasurements.OrderBy(x => x.Description).ToListAsync();
        //    model.VitalSignSetups = totalvs;
        //    var deletevs = await _context.VitalSignSetups.FindAsync(VitalSignSetUpId);
        //    try
        //    {

        //        if (deletevs != null)
        //        {
        //            _context.VitalSignSetups.Remove(deletevs);
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
        //        ModelState.AddModelError("", $"Vital Sign ({deletevs.Desctiption}) can't delete!.These table are use in another Table");
        //        return View("VitalSignSetUp", model);
        //    }
        //    return RedirectToAction(nameof(VitalSignSetup));

        //    //return View();
        //}

        //private async Task<bool> VSExists(string description, int? VitalSignSetUpId)
        //{
        //    var unit = new VitalSignSetup();
        //    if (VitalSignSetUpId == null || VitalSignSetUpId.Value == 0)
        //    {
        //        unit = await _context.VitalSignSetups
        //            .Where(x => x.Description == description).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        unit = await _context.VitalSignSetups
        //            .Where(x => x.VitalSignSetupId != VitalSignSetUpId && x.Description == description).FirstOrDefaultAsync();
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

        public JsonResult Add(string name, int depname)
        {
            DateTime cDate = DateTime.UtcNow.AddMinutes(390);
            DateTime uDate = DateTime.UtcNow.AddMinutes(390);
            Unit unit = new Unit();
            unit.DepartmentId = depname;
            unit.UnitName = name;
            unit.CreatedDate = cDate;
            unit.UpdatedDate = uDate;
            _context.Units.Add(unit);
            _context.SaveChanges();

            var UnitList = _context.Units.ToList();
            return Json(UnitList.LastOrDefault());
        }
        //[HttpGet]
        //[Authorize(Policy = "VitalSignsSetupView")]
        //public async Task<IActionResult> VitalSignSetup(int? VitalSignSetupId, string btnSubmit)
        //{
        //    ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
        //    VitalSignSetUpViewModel model = new VitalSignSetUpViewModel();
        //    var totalVS = _context.VitalSignSetups.Include(x => x.UnitOfMeasurement).OrderBy(a => a.Desctiption).ToList();
        //    var current = totalVS.Where(a => a.VitalSignSetupId == VitalSignSetupId).FirstOrDefault();
        //    model.UnitOfMeasurements = await _context.UnitOfMeasurements.OrderBy(x => x.Description).ToListAsync();
        //    model.VitalSignSetups = totalVS;
        //    btnSubmit = btnSubmit.ToLower().Trim();
        //    if (VitalSignSetupId == null || VitalSignSetupId.Value == 0)
        //    {

        //        model.VitalSignSetup = new VitalSignSetup();
        //        return View(model);
        //    }
        //    else
        //    {
        //        var toUpdateVS = await _context.VitalSignSetups.Include(x => x.UnitOfMeasurement)
        //                .Where(x => x.VitalSignSetupId == VitalSignSetupId.Value).FirstOrDefaultAsync();

        //        List<VitalSignSetup> lst = new List<VitalSignSetup>();
        //        lst = await _context.VitalSignSetups.Where(a => a.VitalSignSetupId == VitalSignSetupId).ToListAsync();
        //        VitalSignSetUpViewModel VitalSignVM = new VitalSignSetUpViewModel()
        //        {
        //            VitalSignSetups = totalVS,
        //            VitalSignSetup = lst.FirstOrDefault(),
        //        };
        //        model.VitalSignSetup = lst.FirstOrDefault();
        //        model.VitalSignSetups = totalVS;
        //        model.UnitOfMeasurementId = toUpdateVS.UnitOfMeasurementId;
        //        model.VitalSignSetupId = toUpdateVS.VitalSignSetupId;
        //        return View(model);
        //    }




        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> VitalSignSetup(VitalSignSetUpViewModel model, string btnSubmit)
        //{
        //    DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
        //    DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
        //    var totalVS = _context.VitalSignSetups.Include(x => x.UnitOfMeasurement).OrderBy(a => a.Desctiption).ToList();
        //    var total = await _context.VitalSignSetups.OrderBy(a => a.Desctiption).ToListAsync();
        //    model.UnitOfMeasurements = await _context.UnitOfMeasurements.OrderBy(x => x.Description).ToListAsync();
        //    model.VitalSignSetups = totalVS;
        //    VitalSignSetup vs = new VitalSignSetup();
        //    btnSubmit = btnSubmit.ToLower().Trim();
        //    if (ModelState.IsValid)
        //    {
        //        if (btnSubmit == "edit")
        //        {
        //            try
        //            {
        //                if (model.VitalSignSetupId == null || model.VitalSignSetupId.Value == 0)
        //                {
        //                    return View("Error");
        //                }
        //                else
        //                {
        //                    if (await VitalSignExists(description: model.VitalSignSetup.Desctiption, VitalSingSetupId: model.VitalSignSetupId))
        //                    {
        //                        ModelState.AddModelError("Desctiption", "Description already exist");
        //                        return View(model);

        //                    }
        //                    else
        //                    {
        //                        var updatedVS = await _context.VitalSignSetups.FindAsync(model.VitalSignSetupId.Value);
        //                        if (updatedVS != null)
        //                        {
        //                            updatedVS.Desctiption = model.VitalSignSetup.Desctiption;
        //                            updatedVS.UnitOfMeasurementId = model.UnitOfMeasurementId;
        //                            updatedVS.MaxRange = model.VitalSignSetup.MaxRange;
        //                            updatedVS.MinRange = model.VitalSignSetup.MinRange;
        //                            updatedVS.UpdatedDate = UpdatedDate;
        //                            _context.VitalSignSetups.Update(updatedVS);
        //                            await _context.SaveChangesAsync();
        //                            TempData["SuccessedAlertMessage"] = "Update Successful";
        //                        }
        //                        else
        //                        {
        //                            return View("Error");
        //                        }
        //                    }
        //                }
        //            }
        //            catch (DbUpdateConcurrencyException)
        //            {
        //                throw;
        //            }

        //        }
        //        else if (btnSubmit == "delete")
        //        {
        //            try
        //            {
        //                if (model.VitalSignSetupId == null || model.VitalSignSetupId.Value == 0)
        //                {
        //                    return View("Error");
        //                }
        //                else
        //                {
        //                    var updatedVS = await _context.VitalSignSetups.FindAsync(model.VitalSignSetupId.Value);
        //                    if (updatedVS != null)
        //                    {

        //                        VitalSignSetup  vsSetup= await _context.VitalSignSetups.FindAsync(model.VitalSignSetupId.Value);
        //                        _context.VitalSignSetups.Remove(vsSetup);
        //                        await _context.SaveChangesAsync();
        //                        TempData["SuccessedAlertMessage"] = "Delete Successful";
        //                    }
        //                    else
        //                    {
        //                        ModelState.AddModelError("VitalSignSetup.Desctiption", "Description already exist");
        //                        VitalSignSetUpViewModel vsView = new VitalSignSetUpViewModel()
        //                        {
        //                            VitalSignSetups = total,

        //                        };
        //                        return View(vsView);
        //                    }
        //                }
        //            }
        //            catch (DbUpdateException ex)
        //            {
        //                ModelState.AddModelError("", "Unit Name can't delete!.These table are use in another Table");
        //                return View(model);
        //            }
        //            return RedirectToAction(nameof(VitalSignSetup));

        //        }
        //        else
        //        {
        //            model.VitalSignSetup.CreatedDate = CreatedDate;
        //            model.VitalSignSetup.UpdatedDate = UpdatedDate;
        //            model.VitalSignSetup.Desctiption = model.VitalSignSetup.Desctiption;
        //            model.VitalSignSetup.MaxRange = model.VitalSignSetup.MaxRange;
        //            model.VitalSignSetup.MinRange = model.VitalSignSetup.MinRange;
        //            model.VitalSignSetup.UnitOfMeasurementId = model.UnitOfMeasurementId;
        //            if (await VitalSignExists(description: model.VitalSignSetup.Desctiption, VitalSingSetupId: null))
        //            {
        //                ModelState.AddModelError("Desctiption", "Description already exist");
        //                return View(model);
        //            }
        //            else
        //            {
        //                await _context.VitalSignSetups.AddAsync(model.VitalSignSetup);
        //                await _context.SaveChangesAsync();
        //                TempData["SuccessedAlertMessage"] = "Save Successful";


        //            }
        //        }
        //        return RedirectToAction(nameof(VitalSignSetup));
        //    }
        //    else
        //    {
        //        model.VitalSignSetup.CreatedDate = CreatedDate;
        //        model.VitalSignSetup.UpdatedDate = UpdatedDate;
        //        model.VitalSignSetup.Desctiption = model.VitalSignSetup.Desctiption;
        //        model.VitalSignSetup.MaxRange = model.VitalSignSetup.MaxRange;
        //        model.VitalSignSetup.MinRange = model.VitalSignSetup.MinRange;  
        //        model.VitalSignSetup.UnitOfMeasurementId = model.UnitOfMeasurementId;
        //        await _context.VitalSignSetups.AddAsync(model.VitalSignSetup);
        //        await _context.SaveChangesAsync();
        //        TempData["SuccessedAlertMessage"] = "Save Successful";
        //        ModelState.AddModelError("VitalSignSetup.Desctiption", "Vital Sign can't create!");

        //        return View(model);

        //    }
        //}
        //private async Task<bool> VitalSignExists(string description, int? VitalSingSetupId)
        //{
        //    var vs = new VitalSignSetup();
        //    if (VitalSingSetupId == null || VitalSingSetupId.Value == 0)
        //    {
        //        vs = await _context.VitalSignSetups
        //            .Where(x => x.Desctiption == description).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        vs = await _context.VitalSignSetups
        //            .Where(x => x.VitalSignSetupId != VitalSingSetupId && x.Desctiption == description).FirstOrDefaultAsync();
        //    }
        //    if (vs == null)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }

        //}

    }

}