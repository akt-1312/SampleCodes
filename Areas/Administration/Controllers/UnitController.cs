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
using HMS.Extensions;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class UnitController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService authorizationService;

        public UnitViewModel UnitViewModel { get; set; }
        public UnitController(ApplicationDbContext context,
                                IAuthorizationService authorizationService)
        {
            _context = context;
            this.authorizationService = authorizationService;
            UnitViewModel = new UnitViewModel()
            {
                DepartmentList = _context.Departments.ToList(),
                UnitList = new List<Unit>(),
            };

        }


        [HttpGet]
        [Authorize(Policy = "UnitView")]
        public async Task<IActionResult> Unit()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            UnitViewModel model = new UnitViewModel();
            var totalUnits = _context.Units.Include(x => x.Department).OrderBy(a => a.UnitName).ToList();
            model.DepartmentList = await _context.Departments.OrderBy(x => x.DepartmentName).ToListAsync();
            model.UnitList = totalUnits;
            return View(model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "UnitView")]
        public async Task<IActionResult> Unit(UnitViewModel model, string btnSubmit, int toDeleteUnitId)
        {
            btnSubmit = btnSubmit.ToLower().Trim();
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalUnits = await _context.Units.Include(x => x.Department).OrderBy(a => a.UnitName).ToListAsync();
            model.DepartmentList = await _context.Departments.OrderBy(x => x.DepartmentName).ToListAsync();
            model.UnitList = totalUnits;
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "UnitDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var deleteUnit = await _context.Units.FindAsync(toDeleteUnitId);
                try
                {
                    if (deleteUnit != null)
                    {
                        _context.Units.Remove(deleteUnit);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(Unit));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Unit Name ({deleteUnit.UnitName}) can't delete!.These table are use in another Table");
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
                        if (!((await authorizationService.AuthorizeAsync(User, "UnitDelete")).Succeeded))
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                        }
                        try
                        {
                            if (model.UnitId == null || model.UnitId.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                //if (await UnitExists(unitName: model.UnitName, unitId: model.UnitId))
                                //{
                                //    ModelState.AddModelError("UnitName", "Unit Name already exist");
                                //    return View(model);

                                //}
                                //else
                                //{
                                    var updatedUnit = await _context.Units.FindAsync(model.UnitId.Value);
                                    if (updatedUnit != null)
                                    {

                                        updatedUnit.UnitName = model.UnitName;
                                        updatedUnit.DepartmentId = model.DepartmentId;
                                        updatedUnit.UpdatedDate = UpdatedDate;
                                        _context.Units.Update(updatedUnit);
                                        await _context.SaveChangesAsync();
                                        TempData["SuccessedAlertMessage"] = "Update Successful";
                                        return RedirectToAction(nameof(Unit));
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
                            return View("Error");
                        }

                    }
                    else
                    {
                        if (!((await authorizationService.AuthorizeAsync(User, "UnitCreate")).Succeeded))
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                        }
                        //if (await UnitExists(unitName: model.UnitName, unitId: null))
                        //{
                        //    ModelState.AddModelError("UnitName", "Unit Name already exist");
                        //    return View(model);
                        //}
                        //else
                        //{
                            try
                            {
                                Unit unit = new Unit();
                                unit.UnitName = model.UnitName;
                                unit.DepartmentId = model.DepartmentId;
                                unit.CreatedDate = CreatedDate;
                                await _context.Units.AddAsync(unit);
                                await _context.SaveChangesAsync();
                                TempData["SuccessedAlertMessage"] = "Save Successful";
                                return RedirectToAction(nameof(Unit));
                            }
                            catch (Exception)
                            {
                                return View("Error");
                            }
                            
                        //}

                    }
                }
                else
                {
                    ModelState.AddModelError("Unit.UnitName", "Unit Name can't create!");

                    return View(model);

                }
            }
        }

        //[HttpPost]
        //[Authorize(Policy = "UnitDelete")]
        //public async Task<IActionResult> DeleteUnit(int? unitId)
        //{
        //    //ModelState.AddModelError("Unit.UnitName", "Unit Name already exist");
        //    UnitViewModel model = new UnitViewModel();

        //}
        //[AllowAnonymous]
        //[AcceptVerbs("Get", "Post")]
        //public async Task<IActionResult> UnitIsInUse(string UnitName, int? UnitId)
        //{
        //    Unit unit = new Unit();
        //    if (UnitId == null || UnitId.Value == 0)
        //    {
        //        unit = await _context.Units.Where(x => x.UnitName.Replace(" ", string.Empty).ToLower().Trim()
        //        == UnitName.StringCompareFormat() || x.Department.DepartmentName.Replace(" ", string.Empty).ToLower().Trim()
        //        == UnitName.StringCompareFormat()).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        unit = await _context.Units.Where(x => x.UnitId != UnitId.Value && (x.UnitName.Replace(" ", string.Empty).ToLower().Trim()
        //        == UnitName.StringCompareFormat() || x.Department.DepartmentName.Replace(" ", string.Empty).ToLower().Trim()
        //        == UnitName.StringCompareFormat())).FirstOrDefaultAsync();
        //    }

        //    if (unit == null)
        //    {
        //        return Json(true);
        //    }
        //    else
        //    {
        //        return Json($"Unit Name {UnitName} is already exist in current table");
        //    }
        //}
        //private async Task<bool> UnitExists(string unitName, int? unitId)
        //{
        //    var unit = new Unit();
        //    if (unitId == null || unitId.Value == 0)
        //    {
        //        unit = await _context.Units
        //            .Where(x => x.UnitName == unitName).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        unit = await _context.Units
        //            .Where(x => x.UnitId != unitId && x.UnitName == unitName).FirstOrDefaultAsync();
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

        #region OldDeptActions
        //[HttpGet]
        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> Unit(int? UnitId, string btnActionName)
        //{
        //    ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
        //    UnitViewModel model = new UnitViewModel();
        //    var totalUnits= _context.Units.Include(x => x.Department).OrderBy(a => a.UnitName).ToList();
        //    var current = totalUnits.Where(a => a.UnitId == UnitId).FirstOrDefault();
        //    model.DepartmentList = await _context.Departments.OrderBy(x => x.DepartmentName).ToListAsync();
        //    model.UnitList = totalUnits;
        //    if (UnitId == null || UnitId.Value == 0)
        //    {

        //        model.Unit = new Unit();
        //        model.BtnActionName = "Create";
        //        return View(model);
        //    }
        //    else
        //    {
        //        var toUpdateUnit = await _context.Units.Include(x => x.Department)
        //                .Where(x => x.UnitId == UnitId.Value).FirstOrDefaultAsync();

        //        List<Unit> lst = new List<Unit>();
        //        lst = await _context.Units.Where(a => a.UnitId == UnitId).ToListAsync();
        //        UnitViewModel moduleVm = new UnitViewModel()
        //        {
        //            UnitList = totalUnits,
        //            Unit = lst.FirstOrDefault(),
        //            BtnActionName = btnActionName,
        //        };
        //        //model.CountryId = toUpdateTownship.State.CountryId;
        //        //model.CountryName = toUpdateTownship.State.Country.Cty_name;
        //        model.DepartmentId = toUpdateUnit.DepartmentId;
        //        model.DepartmentName = toUpdateUnit.Department.DepartmentName;
        //        model.UnitId = toUpdateUnit.UnitId;
        //        model.UnitName = toUpdateUnit.UnitName;

        //        if (btnActionName.ToLower().Trim() == "edit")
        //        {
        //            model.BtnActionName = "Edit";
        //        }
        //        else
        //        {
        //            model.BtnActionName = "Delete";
        //        }
        //        return View(model);
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Unit(UnitViewModel model, string btnSubmit)
        //{
        //    DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
        //    DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
        //    var totalUnits = _context.Units.Include(x => x.Department).OrderBy(a => a.UnitName).ToList();
        //    var total = await _context.Units.OrderBy(a => a.UnitName).ToListAsync();
        //    model.DepartmentList = await _context.Departments.OrderBy(x => x.DepartmentName).ToListAsync();
        //    model.UnitList = totalUnits;
        //    Unit unit = new Unit();
        //    btnSubmit = btnSubmit.ToLower().Trim();
        //    if (ModelState.IsValid)
        //    {
        //        //var NameExist = await _context.Townships.Where(x => x.Tsp_name == model.Township.Tsp_name).FirstOrDefaultAsync();
        //        if (btnSubmit == "edit")
        //        {
        //            try
        //            {
        //                if (model.UnitId == null || model.UnitId.Value == 0)
        //                {
        //                    return View("Error");
        //                }
        //                else
        //                {
        //                    if (await UnitExists(unitName: model.UnitName, unitId: model.UnitId))
        //                    {
        //                        ModelState.AddModelError("UnitName", "Unit Name already exist");
        //                        return View(model);

        //                    }
        //                    else
        //                    {
        //                        var updatedUnit= await _context.Units.FindAsync(model.UnitId.Value);
        //                        if (updatedUnit != null)
        //                        {
        //                            updatedUnit.UnitName = model.UnitName;
        //                            updatedUnit.DepartmentId = model.DepartmentId;
        //                            updatedUnit.UpdatedDate = UpdatedDate;
        //                            _context.Units.Update(updatedUnit);
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
        //                if (model.UnitId == null || model.UnitId.Value == 0)
        //                {
        //                    return View("Error");
        //                }
        //                else
        //                {
        //                    var updatedUnit = await _context.Units.FindAsync(model.UnitId.Value);
        //                    if (updatedUnit != null)
        //                    {

        //                        Unit updatedState = await _context.Units.FindAsync(model.UnitId.Value);
        //                        _context.Units.Remove(updatedState);
        //                        await _context.SaveChangesAsync();
        //                        TempData["SuccessedAlertMessage"] = "Delete Successful";
        //                    }



        //                    else
        //                    {

        //                        ModelState.AddModelError("Unit.UnitName", "Unit Name already exist");
        //                        UnitViewModel unitview = new UnitViewModel()
        //                        {
        //                            UnitList = total,

        //                        };
        //                        return View(unitview);
        //                        //return View("Error");
        //                    }
        //                }
        //            }
        //            catch (DbUpdateException ex)
        //            {
        //                ModelState.AddModelError("", "Unit Name can't delete!.These table are use in another Table");
        //                return View(model);
        //            }
        //            return RedirectToAction(nameof(Unit));

        //        }
        //        else
        //        {
        //            if (await UnitExists(unitName: model.UnitName, unitId: null))
        //            {
        //                ModelState.AddModelError("UnitName", "Unit Name already exist");
        //                return View(model);
        //            }
        //            else
        //            {
        //                unit.UnitName = model.UnitName;
        //                unit.DepartmentId = model.DepartmentId;
        //                unit.CreatedDate = CreatedDate;
        //                await _context.Units.AddAsync(unit);
        //                await _context.SaveChangesAsync();
        //                TempData["SuccessedAlertMessage"] = "Save Successful";


        //            }
        //        }
        //        return RedirectToAction(nameof(Unit));
        //    }
        //    else
        //    {
        //        ModelState.AddModelError("Unit.UnitName", "Unit Name can't create!");

        //        return View(model);

        //    }



        //}
        //private async Task<bool> UnitExists(string unitName, int? unitId)
        //{
        //    var unit = new Unit();
        //    if (unitId == null || unitId.Value == 0)
        //    {
        //        unit = await _context.Units
        //            .Where(x => x.UnitName == unitName).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        unit = await _context.Units
        //            .Where(x => x.UnitId != unitId && x.UnitName == unitName).FirstOrDefaultAsync();
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
        //public JsonResult Add(string name,int depname)
        //{
        //    DateTime cDate = DateTime.UtcNow.AddMinutes(390);
        //    DateTime uDate = DateTime.UtcNow.AddMinutes(390);
        //    Unit unit = new Unit();
        //    unit.DepartmentId = depname;
        //    unit.UnitName = name;
        //    unit.CreatedDate = cDate;
        //    unit.UpdatedDate = uDate;
        //    _context.Units.Add(unit);
        //    _context.SaveChanges();

        //    var UnitList = _context.Units.ToList();
        //    return Json(UnitList.LastOrDefault());
        //}
        #endregion
    }

}
