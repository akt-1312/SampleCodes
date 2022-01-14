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
using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Authorization;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class DoctorInfoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService authorizationService;
        public DoctorInfoViewModel doctorInfoVM { get; set; }
        public DoctorInfoController(ApplicationDbContext context,
                                    IAuthorizationService authorizationService)
        {
            _context = context;
            this.authorizationService = authorizationService;
            doctorInfoVM = new DoctorInfoViewModel()
            {
                //DoctorInfo = new DoctorInfo(),
                DoctorInfos = new List<DoctorInfo>(),
                Unit = new Unit(),
                Units = new List<Unit>(),
                GetUnits = new List<Unit>(),
                Department = new Department(),
                Departments = new List<Department>(),
                UnitDepartmentViewModels = new List<UnitDepartmentViewModel>(),
                DoctorDeptUnitViewModels=new List<DoctorDeptUnitViewModel>(),
            };
        }
        [HttpGet]
        [Authorize(Policy = "DoctorInfoView")]
        public async Task<IActionResult> DoctorInfo(int doctorInfoId)
        {
            #region Old
            //ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            //DoctorInfoViewModel doctorInfoVM = new DoctorInfoViewModel();
            //List<UnitDepartmentViewModel> unitDepartmentVm = new List<UnitDepartmentViewModel>();

            //var totalUnitOfDoctor = _context.UnitsOfDoctors.ToList();
            //var totalDepartments = _context.Departments.ToList();
            //var totalInfos = _context.DoctorInfos.Where(x => x.IsActiveDoctor == true).ToList();
            //var currentInfo = totalInfos.Where(a => a.DoctorInfoId == doctorInfoId && a.IsActiveDoctor == true).FirstOrDefault();
            //ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            //ViewData["DoctorInfos"] = _context.DoctorInfos.OrderByDescending(x => x.DoctorInfoId).ToList();
            //var lstUnits = await _context.Units.Include(x => x.Department).ToListAsync();
            //var groupByList = lstUnits.GroupBy(a => a.DepartmentId).Select(b => b.FirstOrDefault()).ToList();

            //var unitOfDoctorsList = await _context.UnitsOfDoctors.Include(x => x.DoctorInfo)
            //    .Include(x => x.Unit).ThenInclude(x => x.Department).ToListAsync();
            //var groupByUnitOfDoctorList = unitOfDoctorsList.GroupBy(x => x.DoctorInfo).ToList()
            //    .Select(x => new DoctorDeptUnitViewModel()
            //    {
            //        DoctorInfo = x.Key,
            //        DeptUnitsInDrInfo = unitOfDoctorsList.Where(y => y.DoctorInfo == x.Key).ToList().GroupBy(a => a.Unit.Department).ToList()
            //    .Select(z => new DeptUnitViewModel()
            //    {
            //        Department = z.Key,
            //        Units = z.ToList().Where(u => u.Unit.Department == z.Key).Select(d => d.Unit).ToList()
            //    }).Distinct().ToList()
            //    }).ToList();



            //if (doctorInfoId == 0)
            //{

            //    doctorInfoVM.DoctorInfos = totalInfos ?? new List<DoctorInfo>();
            //    doctorInfoVM.DoctorInfo = currentInfo ?? new DoctorInfo();
            //    doctorInfoVM.Units = groupByList ?? new List<Unit>();
            //    doctorInfoVM.GetUnits = lstUnits ?? new List<Unit>();
            //    doctorInfoVM.Departments = totalDepartments ?? new List<Department>();
            //    doctorInfoVM.UnitsOfDoctors = totalUnitOfDoctor ?? new List<UnitsOfDoctor>();
            //    foreach (var item in totalDepartments)
            //    {
            //        UnitDepartmentViewModel unitVm = new UnitDepartmentViewModel();
            //        DoctorInfoDepartmentViewModel departmentVm = new DoctorInfoDepartmentViewModel()
            //        {
            //            Department = item ?? new Department(),
            //            IsCheck = false
            //        };

            //        List<DoctorInfoUnitViewModel> drInfoUnits = new List<DoctorInfoUnitViewModel>();
            //        var unitList = await _context.Units.Where(x => x.DepartmentId == item.DepartmentId).ToListAsync();
            //        if (unitList != null && unitList.Count > 0)
            //        {
            //            foreach (var unit in unitList)
            //            {
            //                DoctorInfoUnitViewModel drInfoUnit = new DoctorInfoUnitViewModel();
            //                drInfoUnit.Unit = unit;
            //                drInfoUnit.IsCheck = false;
            //                drInfoUnits.Add(drInfoUnit);
            //            }
            //        }

            //        unitVm.DrInfoDepartment = departmentVm;
            //        unitVm.DrInfoUnits = drInfoUnits;

            //        unitDepartmentVm.Add(unitVm);
            //    }

            //    doctorInfoVM.UnitDepartmentViewModels = unitDepartmentVm ?? new List<UnitDepartmentViewModel>();
            //}
            //else
            //{
            //    List<DoctorInfo> lstInfos = new List<DoctorInfo>();
            //    lstInfos = await _context.DoctorInfos.Where(a => a.DoctorInfoId == doctorInfoId).ToListAsync();
            //    List<Unit> UnitList = new List<Unit>();
            //    UnitList = _context.Units.ToList();
            //    doctorInfoVM = new DoctorInfoViewModel()
            //    {
            //        DoctorInfos = lstInfos ?? new List<DoctorInfo>(),
            //        DoctorInfo = lstInfos.FirstOrDefault(),
            //        Departments = totalDepartments ?? new List<Department>(),
            //        Units = UnitList ?? new List<Unit>(),
            //        GetUnits = UnitList ?? new List<Unit>(),
            //        UnitsOfDoctors = totalUnitOfDoctor ?? new List<UnitsOfDoctor>(),
            //    };

            //    var unitOfDoctorEditList = await _context.UnitsOfDoctors
            //        .Where(x => x.DoctorId == doctorInfoId).Include(x => x.Unit)
            //        .ThenInclude(x => x.Department).Include(x => x.DoctorInfo)
            //        .ToListAsync() ?? new List<UnitsOfDoctor>();

            //    List<UnitDepartmentViewModel> unitDepartmentViewModels = new List<UnitDepartmentViewModel>();
            //    var unitsListGroupByDept = lstUnits.GroupBy(x => x.Department).OrderBy(x => x.Key.DepartmentName).ToList();
            //    foreach (var item in unitsListGroupByDept)
            //    {
            //        UnitDepartmentViewModel unitDepartmentViewModel = new UnitDepartmentViewModel();
            //        unitDepartmentViewModel.DrInfoDepartment = new DoctorInfoDepartmentViewModel();
            //        List<DoctorInfoUnitViewModel> doctorInfoUnitViewModels = new List<DoctorInfoUnitViewModel>();

            //        unitDepartmentViewModel.DrInfoDepartment.Department = item.Key;
            //        unitDepartmentViewModel.DrInfoDepartment.IsCheck = unitOfDoctorEditList
            //            .Where(x => x.Unit.DepartmentId == item.Key.DepartmentId).Any();
            //        unitDepartmentViewModel.DrInfoUnits = item.ToList().Select(x => new DoctorInfoUnitViewModel()
            //        {
            //            Unit = x,
            //            IsCheck = unitOfDoctorEditList.Where(y => y.UnitId == x.UnitId).Any()
            //        }).ToList();
            //        unitDepartmentViewModels.Add(unitDepartmentViewModel);
            //    }

            //    doctorInfoVM.UnitDepartmentViewModels = unitDepartmentViewModels;

            //}
            //doctorInfoVM.DoctorDeptUnitViewModels = groupByUnitOfDoctorList;
            //return View(doctorInfoVM);
            #endregion

            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            DoctorInfoViewModel model = new DoctorInfoViewModel();
            List<UnitDepartmentViewModel> unitDepartmentVm = new List<UnitDepartmentViewModel>();
            var totalUnitOfDoctor = _context.UnitsOfDoctors.ToList();
            var totalDepartments = _context.Departments.ToList();
            var totalInfos = _context.DoctorInfos.Where(x => x.IsActiveDoctor == true).ToList();
            var totalUnits = _context.Units.Include(x => x.Department).OrderBy(a => a.UnitName).ToList();
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewData["DoctorInfos"] = _context.DoctorInfos.OrderByDescending(x => x.DoctorInfoId).ToList();
            var lstUnits = await _context.Units.Include(x => x.Department).ToListAsync();
            var groupByList = lstUnits.GroupBy(a => a.DepartmentId).Select(b => b.FirstOrDefault()).ToList();

            var unitOfDoctorsList = await _context.UnitsOfDoctors.Include(x => x.DoctorInfo)
                .Include(x => x.Unit).ThenInclude(x => x.Department).ToListAsync();
            var groupByUnitOfDoctorList = unitOfDoctorsList.GroupBy(x => x.DoctorInfo).ToList()
                .Select(x => new DoctorDeptUnitViewModel()
                {
                    DoctorInfo = x.Key,
                    DeptUnitsInDrInfo = unitOfDoctorsList.Where(y => y.DoctorInfo == x.Key).ToList().GroupBy(a => a.Unit.Department).ToList()
                .Select(z => new DeptUnitViewModel()
                {
                    Department = z.Key,
                    Units = z.ToList().Where(u => u.Unit.Department == z.Key).Select(d => d.Unit).ToList()
                }).Distinct().ToList()
                }).ToList();
            foreach (var item in totalDepartments)
            {
                UnitDepartmentViewModel unitVm = new UnitDepartmentViewModel();
                DoctorInfoDepartmentViewModel departmentVm = new DoctorInfoDepartmentViewModel()
                {
                    Department = item ?? new Department(),
                    IsCheck = false
                };

                List<DoctorInfoUnitViewModel> drInfoUnits = new List<DoctorInfoUnitViewModel>();
                var unitList = await _context.Units.Where(x => x.DepartmentId == item.DepartmentId).ToListAsync();
                if (unitList != null && unitList.Count > 0)
                {
                    foreach (var unit in unitList)
                    {
                        DoctorInfoUnitViewModel drInfoUnit = new DoctorInfoUnitViewModel();
                        drInfoUnit.Unit = unit;
                        drInfoUnit.IsCheck = false;
                        drInfoUnits.Add(drInfoUnit);
                    }
                }

                unitVm.DrInfoDepartment = departmentVm;
                unitVm.DrInfoUnits = drInfoUnits;

                unitDepartmentVm.Add(unitVm);
            }

            doctorInfoVM.UnitDepartmentViewModels = unitDepartmentVm ?? new List<UnitDepartmentViewModel>();
            doctorInfoVM.DoctorDeptUnitViewModels = groupByUnitOfDoctorList;
            return View(doctorInfoVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DoctorInfoView")]
        public async Task<IActionResult> DoctorInfo(DoctorInfoViewModel model, List<string> CheckUnit, string btnSubmit, int toDeleteDRId)
        {
            #region Old
            ////    var totalUnitOfDoctor = _context.UnitsOfDoctors.ToList();
            ////    var totalDepartments = _context.Departments.ToList();
            ////    var totalInfos = _context.DoctorInfos.ToList();
            ////    var currentInfo = new DoctorInfo();
            ////    ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ////    ViewData["DoctorInfos"] = _context.DoctorInfos.OrderByDescending(x => x.DoctorInfoId).ToList();
            ////    var lstUnits = await _context.Units.ToListAsync();
            ////    var groupByList = lstUnits.GroupBy(a => a.DepartmentId).Select(b => b.FirstOrDefault()).ToList();
            ////    model.DoctorInfos = totalInfos ?? new List<DoctorInfo>();
            ////    model.Units = lstUnits ?? new List<Unit>();
            ////    model.GetUnits = lstUnits ?? new List<Unit>();
            ////    model.Departments = totalDepartments ?? new List<Department>();
            ////    model.UnitsOfDoctors = totalUnitOfDoctor ?? new List<UnitsOfDoctor>();
            ////    var checkedDoctorUnitsList = model.UnitDepartmentViewModels.Where(x => x.oDepartment.IsCheck).ToList();
            ////    if (ModelState.IsValid)
            ////    {
            ////        var DoctorNameExist = await _context.DoctorInfos.Where(x => x.DoctorName == model.DoctorInfo.DoctorName).FirstOrDefaultAsync();
            ////        if (model.BtnActionName != "Delete")
            ////        {
            ////            if (model.BtnActionName == "Edit" && checkedDoctorUnitsList.Count() != 0)
            ////            {
            ////                try
            ////                {
            ////                    var doctorEdit = _context.DoctorInfos.Where(p => p.DoctorInfoId == model.DoctorInfo.DoctorInfoId).Single();
            ////                    var unitOfDoctorEdit = _context.UnitsOfDoctors.Where(p => p.DoctorId == model.DoctorInfo.DoctorInfoId).ToList();
            ////                    List<UnitsOfDoctor> unitsOfDoctors = new List<UnitsOfDoctor>();
            ////                    _context.UnitsOfDoctors.RemoveRange(unitOfDoctorEdit);
            ////                    await _context.SaveChangesAsync();

            ////                    foreach (var item in checkedDoctorUnitsList)
            ////                    {
            ////                        foreach (var unit in item.DrInfoUnits.Where(x => x.IsCheck))
            ////                        {
            ////                            UnitsOfDoctor unitsOfDoctor = new UnitsOfDoctor();
            ////                            unitsOfDoctor.DoctorId = model.DoctorInfo.DoctorInfoId;
            ////                            unitsOfDoctor.UnitId = unit.Unit.UnitId;
            ////                            unitsOfDoctors.Add(unitsOfDoctor);
            ////                        }
            ////                    }
            ////                    var doctoredit = _context.DoctorInfos.Where(p => p.DoctorInfoId == model.DoctorInfo.DoctorInfoId).Single();
            ////                    doctoredit.Email = model.DoctorInfo.Email;
            ////                    doctoredit.DoctorName = model.DoctorInfo.DoctorName;

            ////                    await _context.UnitsOfDoctors.AddRangeAsync(unitsOfDoctors);
            ////                    await _context.SaveChangesAsync();
            ////                    _context.DoctorInfos.UpdateRange(doctoredit);
            ////                    await _context.SaveChangesAsync();
            ////                    TempData["SuccessedAlertMessage"] = "Update Successful";
            ////                    return RedirectToAction(nameof(DoctorInfo));
            ////                }
            ////                catch
            ////                {
            ////                    if (!DoctorExists(model.DoctorInfo.DoctorInfoId))
            ////                    {
            ////                        return NotFound();
            ////                    }
            ////                    else
            ////                    {
            ////                        throw;
            ////                    }
            ////                }
            ////            }
            ////            else /*if (model.BtnActionName == "Create" && checkedDoctorUnitsList != null)*/
            ////            {
            ////                model.DoctorInfo.CreatedDate = DateTime.UtcNow.AddMinutes(390);
            ////                model.DoctorInfo.UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            ////                await _context.DoctorInfos.AddAsync(model.DoctorInfo);
            ////                await _context.SaveChangesAsync();
            ////                TempData["SuccessedAlertMessage"] = "Create Successful";
            ////                List<UnitsOfDoctor> unitsOfDoctors = new List<UnitsOfDoctor>();
            ////                foreach (var item in checkedDoctorUnitsList)
            ////                {
            ////                    foreach (var unit in item.DrInfoUnits.Where(x => x.IsCheck))
            ////                    {
            ////                        UnitsOfDoctor unitsOfDoctor = new UnitsOfDoctor();

            ////                        unitsOfDoctor.DoctorId = model.DoctorInfo.DoctorInfoId;
            ////                        unitsOfDoctor.UnitId = unit.Unit.UnitId;
            ////                        unitsOfDoctors.Add(unitsOfDoctor);
            ////                    }
            ////                }

            ////                await _context.UnitsOfDoctors.AddRangeAsync(unitsOfDoctors);
            ////                await _context.SaveChangesAsync();
            ////                return RedirectToAction(nameof(DoctorInfo));
            ////            }
            ////            //else
            ////            //{
            ////            //    ModelState.AddModelError("DoctorInfo.DoctorName", "Please Check Departments and Units!");
            ////            //    var doctorEdit = _context.DoctorInfos.Where(p => p.DoctorInfoId == model.DoctorInfo.DoctorInfoId).Single();
            ////            //    return RedirectToAction(nameof(DoctorInfo), new { doctorInfoId = model.DoctorInfo.DoctorInfoId });
            ////            //}
            ////        }
            ////        else
            ////        {
            ////            if (model.BtnActionName == "Delete")
            ////            {
            ////                var doctorInfo = _context.DoctorInfos
            ////                   .Where(a => a.DoctorInfoId == model.DoctorInfo.DoctorInfoId).Single();
            ////                var UnitDoc = await _context.UnitsOfDoctors.Where(u => u.DoctorId == model.DoctorInfo.DoctorInfoId).ToListAsync();
            ////                if (UnitDoc != null)
            ////                {
            ////                    _context.UnitsOfDoctors.RemoveRange(UnitDoc);
            ////                    _context.SaveChanges();
            ////                }
            ////                doctorInfo.IsActiveDoctor = false;
            ////                _context.DoctorInfos.Remove(doctorInfo);
            ////                await _context.SaveChangesAsync();
            ////                TempData["SuccessedAlertMessage"] = "Delete Successful";
            ////                return RedirectToAction(nameof(DoctorInfo));
            ////            }
            ////            else
            ////            {
            ////                try
            ////                {
            ////                    ModelState.AddModelError("DoctorInfo.DoctorName", "Doctor Name already exist");
            ////                    return View(model);
            ////                }
            ////                catch (DbUpdateConcurrencyException)
            ////                {
            ////                    if (!DoctorExists(model.DoctorInfo.DoctorInfoId))
            ////                    {
            ////                        return NotFound();
            ////                    }
            ////                    else
            ////                    {
            ////                        throw;
            ////                    }
            ////                }
            ////            }
            ////        }
            ////    }

            ////    else
            ////    {
            ////        model.DoctorInfo.CreatedDate = DateTime.UtcNow.AddMinutes(390);
            ////        model.DoctorInfo.UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            ////        await _context.DoctorInfos.AddAsync(model.DoctorInfo);
            ////        await _context.SaveChangesAsync();
            ////        foreach (var unitId in CheckUnit)
            ////        {
            ////            var unitListOfDoctor = new UnitsOfDoctor();
            ////            unitListOfDoctor.DoctorId = model.DoctorInfo.DoctorInfoId;
            ////            unitListOfDoctor.UnitId = int.Parse(unitId);
            ////            _context.UnitsOfDoctors.Add(unitListOfDoctor);
            ////        }
            ////        await _context.SaveChangesAsync();

            ////        return RedirectToAction(nameof(DoctorInfo));
            ////    }
            ////}

            //[HttpPost]

            //private bool DoctorExists(int id)
            //{
            //    return _context.DoctorInfos.Any(e => e.DoctorInfoId == id);
            //}
            //public JsonResult AddDoctor(int id)
            //{
            //    List<Unit> UnitList = new List<Unit>();
            //    UnitList = _context.Units.Where(m => m.DepartmentId == id).ToList();
            //    return Json(UnitList);
            //}
            #endregion
            btnSubmit = btnSubmit.ToLower().Trim();
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalUnitOfDoctor = _context.UnitsOfDoctors.ToList();
            var totalDepartments = _context.Departments.ToList();
            var totalInfos = _context.DoctorInfos.ToList();
            var currentInfo = new DoctorInfo();
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewData["DoctorInfos"] = _context.DoctorInfos.OrderByDescending(x => x.DoctorInfoId).ToList();
            var lstUnits = await _context.Units.ToListAsync();
            var groupByList = lstUnits.GroupBy(a => a.DepartmentId).Select(b => b.FirstOrDefault()).ToList();
            model.DoctorInfos = totalInfos ?? new List<DoctorInfo>();
            model.Units = lstUnits ?? new List<Unit>();
            model.GetUnits = lstUnits ?? new List<Unit>();
            model.Departments = totalDepartments ?? new List<Department>();
            model.UnitsOfDoctors = totalUnitOfDoctor ?? new List<UnitsOfDoctor>();
            var checkedDoctorUnitsList = model.UnitDepartmentViewModels.Where(x => x.DrInfoDepartment.IsCheck).ToList();
            var totalDrInfos = await _context.DoctorInfos.OrderBy(a => a.DoctorName).ToListAsync();

            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "DoctorInfoDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var deleteDrInfo = await _context.DoctorInfos.FindAsync(toDeleteDRId);
                try
                {
                    if (deleteDrInfo != null)
                    {
                        _context.DoctorInfos.Remove(deleteDrInfo);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(DoctorInfo));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Doctor Name ({deleteDrInfo.DoctorName}) can't delete!.These table are use in another Table");
                    return View(model);
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (btnSubmit == "update")
                    {
                        //if (!((await authorizationService.AuthorizeAsync(User, "DoctorInfoDelete")).Succeeded))
                        //{
                        //    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                        //}
                        //try
                        //{
                        //    if (model.DoctorInfoId == null || model.DoctorInfoId.Value == 0)
                        //    {
                        //        return View("Error");
                        //    }
                        //    else
                        //    {
                                
                        //        var updatedDr = await _context.DoctorInfos.FindAsync(model.DoctorInfoId.Value);
                        //        if (updatedDr != null)
                        //        {

                        //            updatedDr.DoctorName = model.DoctorName;
                        //            updatedDr.Email = model.Email;
                        //            updatedDr.UpdatedDate = UpdatedDate;
                        //            _context.Units.Update(updatedDr);
                        //            await _context.SaveChangesAsync();
                        //            TempData["SuccessedAlertMessage"] = "Update Successful";
                        //            return RedirectToAction(nameof(DoctorInfo));
                        //        }
                        //        else
                        //        {
                        //            return View("Error");
                        //        }
                        //    }
                        //}
                        //catch (DbUpdateConcurrencyException)
                        //{
                            return View("Error");
                        //}

                    }
                    else
                    {
                        if (!((await authorizationService.AuthorizeAsync(User, "DoctorInfoCreate")).Succeeded))
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                        }
                        try
                        {
                            DoctorInfo dr = new DoctorInfo();
                            dr.DoctorName = model.DoctorName;
                            dr.Email = model.Email;
                            dr.CreatedDate = CreatedDate;
                            dr.UpdatedDate = UpdatedDate;
                            await _context.DoctorInfos.AddAsync(dr);
                            await _context.SaveChangesAsync();
                            DoctorInfo doctorInfo = new DoctorInfo();
                            TempData["SuccessedAlertMessage"] = "Save Successful";
                            List<UnitsOfDoctor> unitsOfDoctors = new List<UnitsOfDoctor>();
                            foreach (var item in checkedDoctorUnitsList)
                            {
                                foreach (var unit in item.DrInfoUnits.Where(x => x.IsCheck))
                                {
                                    UnitsOfDoctor unitsOfDoctor = new UnitsOfDoctor();

                                    unitsOfDoctor.DoctorId = dr.DoctorInfoId/*_context.DoctorInfos.Select(x=>x.DoctorInfoId).Last()*/;
                                    unitsOfDoctor.UnitId = unit.Unit.UnitId;
                                    unitsOfDoctors.Add(unitsOfDoctor);
                                }
                            }

                            await _context.UnitsOfDoctors.AddRangeAsync(unitsOfDoctors);
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(DoctorInfo));
                        }
                        catch (Exception)
                        {
                            return View("Error");
                        }

                    }

                }
                else
                {
                    ModelState.AddModelError("Unit.UnitName", "Unit Name can't create!");

                    return View(model);

                }
            }
            return View(model);
        }
    }
}
