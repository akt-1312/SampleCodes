using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Administration;
using HMS.Models.ViewModels;
using HMS.Models.ViewModels.Administration;
using HMS.Models.ViewModels.CommonViewModal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Administration.Controllers
{
    [Authorize]
    [Area("Administration")]
    public class DoctorDutyController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IAuthorizationService authorizationService;

        public DoctorDutyController(ApplicationDbContext db, IAuthorizationService authorizationService)
        {
            this.db = db;
            this.authorizationService = authorizationService;
        }

        //[BindProperty]
        //public DoctorDuty DoctorDuty { get; set; }

        [Authorize(Policy = "DoctorDutyView")]
        [HttpGet]
        public async Task<IActionResult> DoctorDuty()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            List<DoctorDuty> doctorDutiesList = new List<DoctorDuty>();
            doctorDutiesList = await db.DoctorDuties.Include(dr => dr.DoctorInfo).Include(d => d.Department)
                                    .Include(u => u.Unit).OrderBy(or => or.DoctorInfo.DoctorName).ToListAsync();

            var drDutiesGroup = (from data in doctorDutiesList
                                 group data by new { data.DoctorId, data.Department.DepartmentName, data.Unit.UnitName } into gp
                                 orderby (gp.Key.DepartmentName)
                                 select new DoctorDutyIndexViewModel
                                 {
                                     DepartmentName = gp.Key.DepartmentName,
                                     DoctorId = gp.Key.DoctorId,
                                     UnitName = gp.Key.UnitName,
                                     DoctorDuties = gp.ToList()
                                 }).ToList();

            DoctorDutyViewModel viewModel = new DoctorDutyViewModel()
            {
                DrDeptUnit = new DrDeptUnitForPartialViewModel
                {
                    DefaultRdoButton = "ByDropdownDoctor",
                    Departments = await db.Departments.OrderBy(d => d.DepartmentName).ToListAsync(),
                },
            };
            foreach (var item in viewModel.DayOfWeek)
            {
                viewModel.NeededData.Add(new DoctorDutyRecodeCreateNeededData()
                {
                    DutyDay = item,
                    IsChecked = false,
                });
            }

            viewModel.DoctorDutyList = drDutiesGroup;
            return View(viewModel);

        }

        [Authorize(Policy = "DoctorDutyView")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoctorDuty(DoctorDutyViewModel model, string btnSubmit, int toDelDrId, int toDelDeptId, int toDelUnitId)
        {
            string actionName = btnSubmit.ToLower().Trim();

            if (actionName == "delete")
            {

                if (!((await authorizationService.AuthorizeAsync(User, "DoctorDutyDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }
                ModelState.Clear();
                try
                {
                    List<DoctorDuty> oldDrDuties = await db.DoctorDuties.Where(x => x.DoctorId == toDelDrId && x.DepartmentId == toDelDeptId
                    && x.UnitId == toDelUnitId).ToListAsync();

                    if (oldDrDuties == null)
                    {
                        ViewBag.ErrorTitle = "Doctor Duty Assigned Data Not Found!";
                        ViewBag.ErrorMessage = "Any doctor duty cannot be found. Please make sure there is any doctor duty exist in this doctor.";
                        return View("Error");
                    }

                    db.DoctorDuties.RemoveRange(oldDrDuties);
                    await db.SaveChangesAsync();

                    TempData["SuccessedAlertMessage"] = "Doctor Duty Delete Successful";
                    return RedirectToAction(nameof(DoctorDuty));
                }
                catch (DbUpdateException)
                {
                    ViewBag.ErrorTitle = "Cannot delete doctor duty";
                    ViewBag.ErrorMessage = $"Doctor duty cannot be deleted because " +
                        $"something went wrong in processing. Please contact IT support Team.";
                    return View("Error");
                }

            }
            else
            {
                model.DrDeptUnit = new DrDeptUnitForPartialViewModel
                {
                    DefaultRdoButton = model.DrDeptUnit.DefaultRdoButton,
                    Departments = await db.Departments.OrderBy(d => d.DepartmentName).ToListAsync(),
                    DoctorId = model.DrDeptUnit.DoctorId,
                    DoctorName = model.DrDeptUnit.DoctorName,
                    DepartmentId = model.DrDeptUnit.DepartmentId,
                    DepartmentName = model.DrDeptUnit.DepartmentName,
                    UnitId = model.DrDeptUnit.UnitId,
                    UnitName = model.DrDeptUnit.UnitName
                };

                List<DoctorDuty> doctorDutiesList = new List<DoctorDuty>();
                doctorDutiesList = await db.DoctorDuties.Include(dr => dr.DoctorInfo).Include(d => d.Department)
                                        .Include(u => u.Unit).OrderBy(or => or.DoctorInfo.DoctorName).ToListAsync();

                var drDutiesGroup = (from data in doctorDutiesList
                                     group data by new { data.DoctorId, data.Department.DepartmentName, data.Unit.UnitName } into gp
                                     orderby (gp.Key.DepartmentName)
                                     select new DoctorDutyIndexViewModel
                                     {
                                         DepartmentName = gp.Key.DepartmentName,
                                         DoctorId = gp.Key.DoctorId,
                                         UnitName = gp.Key.UnitName,
                                         DoctorDuties = gp.ToList()
                                     });
                if (drDutiesGroup != null)
                {
                    model.DoctorDutyList = drDutiesGroup;
                }
                else
                {
                    model.DoctorDutyList = new List<DoctorDutyIndexViewModel>();
                }

                if (actionName == "create")
                {
                    if (!((await authorizationService.AuthorizeAsync(User, "DoctorDutyCreate")).Succeeded))
                    {
                        return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                    }
                    List<DoctorDuty> lstDoctorDuties = new List<DoctorDuty>();
                    try
                    {
                        List<DoctorDuty> oldDrDuties = await db.DoctorDuties.Where(x => x.DoctorId == model.DrDeptUnit.DoctorId && x.DepartmentId == model.DrDeptUnit.DepartmentId
                        && x.UnitId == model.DrDeptUnit.UnitId).ToListAsync();

                        if (oldDrDuties.Count > 0)
                        {
                            ModelState.AddModelError("", "This Doctor in this department of unit is already assigned.");
                            return View(model);
                        }
                    }
                    catch (Exception)
                    {
                        return View("Error");
                    }

                    foreach (var item in model.NeededData)
                    {
                        if (item.IsChecked && (item.DutyStartTime1.HasValue && item.DutyEndTime1.HasValue) || (item.DutyStartTime2.HasValue && item.DutyEndTime2.HasValue))
                        {
                            lstDoctorDuties.Add(new DoctorDuty()
                            {
                                DoctorId = model.DrDeptUnit.DoctorId,
                                DepartmentId = model.DrDeptUnit.DepartmentId,
                                UnitId = model.DrDeptUnit.UnitId,
                                DutyDay = item.DutyDay,
                                DutyStartTime1 = item.DutyStartTime1,
                                DutyEndTime1 = item.DutyEndTime1,
                                DutyStartTime2 = item.DutyStartTime2,
                                DutyEndTime2 = item.DutyEndTime2,
                                IntervalGapForPatient = model.TimeGap
                            });
                        }
                        if (item.IsChecked && (item.DutyStartTime1.HasValue && item.DutyEndTime1 == null) || (item.DutyEndTime1.HasValue && item.DutyStartTime1 == null)
                            || (item.DutyStartTime2.HasValue && item.DutyEndTime2 == null) || (item.DutyEndTime2.HasValue && item.DutyStartTime2 == null))
                        {
                            ModelState.AddModelError("", "Invalid doctor duty assign value");
                            if (model.DrDeptUnit.DoctorId == 0)
                            {
                                ModelState.AddModelError("", "Choose assigned doctor name.");
                            }
                            return View(model);
                        }
                    }
                    if (lstDoctorDuties.Count > 0)
                    {
                        if (model.DrDeptUnit.DoctorId == 0)
                        {
                            ModelState.AddModelError("", "Choose assigned doctor name.");
                            return View(model);
                        }
                        try
                        {
                            await db.DoctorDuties.AddRangeAsync(lstDoctorDuties);
                            await db.SaveChangesAsync();
                        }
                        catch (DbUpdateException)
                        {
                            ViewBag.ErrorTitle = "Cannot save doctor duty";
                            ViewBag.ErrorTitle = $"Doctor {model.DrDeptUnit.DoctorName} cannot assign duty because " +
                                $"something went wrong in processing. Please contact IT support Team.";
                            return View("Error");
                        }

                        TempData["SuccessedAlertMessage"] = "Doctor Duty Assigned Successful";
                        return RedirectToAction(nameof(DoctorDuty));

                    }
                    else
                    {
                        if (model.DrDeptUnit.DoctorId == 0)
                        {
                            ModelState.AddModelError("", "Choose assigned doctor name.");
                        }
                        if (model.NeededData.Where(x => x.IsChecked).ToList().Count == 0)
                        {
                            ModelState.AddModelError("", "At least one duty must assign.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid doctor duty assign value");
                        }
                        return View(model);
                    }
                }
                else
                {
                    if (!((await authorizationService.AuthorizeAsync(User, "DoctorDutyUpdate")).Succeeded))
                    {
                        return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                    }
                    List<DoctorDuty> lstDoctorDuties = new List<DoctorDuty>();
                    try
                    {
                        List<DoctorDuty> oldDrDuties = await db.DoctorDuties.Where(x => x.DoctorId == model.DrDeptUnit.DoctorId && x.DepartmentId == model.DrDeptUnit.DepartmentId
                        && x.UnitId == model.DrDeptUnit.UnitId).ToListAsync();

                        db.DoctorDuties.RemoveRange(oldDrDuties);
                    }
                    catch (Exception)
                    {
                        return View("Error");
                    }

                    foreach (var item in model.NeededData)
                    {
                        if (item.IsChecked && (item.DutyStartTime1.HasValue && item.DutyEndTime1.HasValue) || (item.DutyStartTime2.HasValue && item.DutyEndTime2.HasValue))
                        {
                            lstDoctorDuties.Add(new DoctorDuty()
                            {
                                DoctorId = model.DrDeptUnit.DoctorId,
                                DepartmentId = model.DrDeptUnit.DepartmentId,
                                UnitId = model.DrDeptUnit.UnitId,
                                DutyDay = item.DutyDay,
                                DutyStartTime1 = item.DutyStartTime1,
                                DutyEndTime1 = item.DutyEndTime1,
                                DutyStartTime2 = item.DutyStartTime2,
                                DutyEndTime2 = item.DutyEndTime2,
                                IntervalGapForPatient = model.TimeGap
                            });
                        }

                        if (item.IsChecked && (item.DutyStartTime1.HasValue && item.DutyEndTime1 == null) || (item.DutyEndTime1.HasValue && item.DutyStartTime1 == null)
                            || (item.DutyStartTime2.HasValue && item.DutyEndTime2 == null) || (item.DutyEndTime2.HasValue && item.DutyStartTime2 == null))
                        {
                            ModelState.AddModelError("", "Invalid doctor duty assign value");
                            if (model.DrDeptUnit.DoctorId == 0)
                            {
                                ModelState.AddModelError("", "Choose assigned doctor name.");
                            }
                            return View(model);
                        }
                    }
                    if (lstDoctorDuties.Count > 0)
                    {
                        if (model.DrDeptUnit.DoctorId == 0)
                        {
                            ModelState.AddModelError("", "Choose assigned doctor name.");
                            return View(model);
                        }
                        else
                        {
                            try
                            {
                                await db.DoctorDuties.AddRangeAsync(lstDoctorDuties);
                                await db.SaveChangesAsync();
                            }
                            catch (DbUpdateException)
                            {
                                ViewBag.ErrorTitle = "Cannot update doctor duty";
                                ViewBag.ErrorTitle = $"Doctor {model.DrDeptUnit.DoctorName} cannot update duty because " +
                                    $"something went wrong in processing. Please contact IT support Team.";
                                return View("Error");
                            }
                            TempData["SuccessedAlertMessage"] = "Update Doctor Duty Success";
                            return RedirectToAction(nameof(DoctorDuty));
                        }

                    }
                    else
                    {
                        //ModelState.AddModelError("", "At least one duty assign is needed.");
                        //return View(model);

                        if (model.DrDeptUnit.DoctorId == 0)
                        {
                            ModelState.AddModelError("", "Choose assigned doctor name.");
                        }
                        if (model.NeededData.Where(x => x.IsChecked).ToList().Count == 0)
                        {
                            ModelState.AddModelError("", "At least one duty must assign.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid doctor duty assign value");
                        }
                        return View(model);
                    }
                }
            }
        }
    }
}