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
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService authorizationService;
        public DepartmentViewModel departmentVM { get; set; }

        public DepartmentController(ApplicationDbContext context,
                                    IAuthorizationService authorizationService)
        {
            _context = context;
            this.authorizationService = authorizationService;
            departmentVM = new DepartmentViewModel()
            {
                Departments = new List<Department>(),
            };
        }

        [HttpGet]
        [Authorize(Policy = "DepartmentView")]
        public async Task<IActionResult> Department()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            DepartmentViewModel model = new DepartmentViewModel();
            model.Departments = await _context.Departments.OrderBy(x => x.DepartmentName).ToListAsync();
            return View(model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DepartmentView")]
        public async Task<IActionResult> Department(DepartmentViewModel model, string btnSubmit,int toDeleteDeptId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            model.Departments = await _context.Departments.OrderBy(x => x.DepartmentName).ToListAsync();
            Department dp = new Department();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "DepartmentDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var delete = await _context.Departments.FindAsync(toDeleteDeptId);
                try
                {
                    if (delete != null)
                    {
                        _context.Departments.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(Department));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Department Name ({delete.DepartmentName}) can't delete!.These table are use in another Table");
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
                        if ((await authorizationService.AuthorizeAsync(User, "DepartmentUpdate")).Succeeded)
                        {
                            try
                            {
                                if (model.DepartmentId == null || model.DepartmentId.Value == 0)
                                {
                                    return View("Error");
                                }
                                else
                                {
                                    //if (await DepartmentExists(DepartmentName: model.DepartmentName))
                                    //{
                                    //    ModelState.AddModelError("DepartmentName", "DepartmentName already exist");
                                    //    return View(model);

                                    //}
                                    //else
                                    //{
                                        var updated = await _context.Departments.FindAsync(model.DepartmentId.Value);
                                        if (updated != null)
                                        {
                                            updated.DepartmentName = model.DepartmentName;
                                            updated.UpdatedDate = UpdatedDate;
                                            _context.Departments.Update(updated);
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
                        if ((await authorizationService.AuthorizeAsync(User, "DepartmentUpdate")).Succeeded)
                        {
                            //if (await DepartmentExists(DepartmentName: model.DepartmentName))
                            //{
                            //    ModelState.AddModelError("DepartmentName", "DepartmentName already exist");
                            //    return View(model);
                            //}
                            //else
                            //{
                                dp.DepartmentName = model.DepartmentName;
                                dp.CreatedDate = CreatedDate;
                                await _context.Departments.AddAsync(dp);
                                await _context.SaveChangesAsync();
                                TempData["SuccessedAlertMessage"] = "Save Successful";
                            //}
                        }
                        else
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                        }
                    }
                    return RedirectToAction(nameof(Department));
                }
                else
                {
                    ModelState.AddModelError("Department.DepartmentName", "Department can't create!");

                    return View(model);

                }
            }
        }

        //[HttpPost]
        //[Authorize(Policy = "DepartmentDelete")]
        //public async Task<IActionResult> DeleteDepartment(int? DepartmentId)
        //{
        //    ModelState.AddModelError("UnitOfMeasurement.Description", "Unit Of Measurement already exist");
        //    DepartmentViewModel model = new DepartmentViewModel();
        //    model.Departments = await _context.Departments.OrderBy(x => x.DepartmentName).ToListAsync();
        //    var deletedept = await _context.Departments.FindAsync(DepartmentId);
        //    try
        //    {

        //        if (deletedept != null)
        //        {
        //            _context.Departments.Remove(deletedept);
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
        //        ModelState.AddModelError("", $"Department ({deletedept.DepartmentName}) can't delete!.These table are use in another Table");
        //        return View("Department", model);
        //    }
        //    return RedirectToAction(nameof(Department));

        //    //return View();
        //}

        //private async Task<bool> DepartmentExists(string DepartmentName)
        //{
        //    var dp = new Department();
        //    if (DepartmentName != null)
        //    {
        //        dp = await _context.Departments
        //            .Where(x => x.DepartmentName == DepartmentName).FirstOrDefaultAsync();
        //    }
        //    if (dp == null)
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
        //public async Task<IActionResult> DepartmentIsInUse(string DepartmentName, int? DepartmentId)
        //{
        //    Department dp = new Department();
        //    if (DepartmentId == null || DepartmentId.Value == 0)
        //    {
        //        dp = await _context.Departments.Where(x => x.DepartmentName.Replace(" ", string.Empty).ToLower().Trim()
        //        == DepartmentName.StringCompareFormat() || x.DepartmentName.Replace(" ", string.Empty).ToLower().Trim()
        //        == DepartmentName.StringCompareFormat()).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        dp = await _context.Departments.Where(x => x.DepartmentId != DepartmentId.Value && (x.DepartmentName.Replace(" ", string.Empty).ToLower().Trim()
        //        == DepartmentName.StringCompareFormat() || x.DepartmentName.Replace(" ", string.Empty).ToLower().Trim()
        //        == DepartmentName.StringCompareFormat())).FirstOrDefaultAsync();
        //    }
        //    if (dp == null)
        //    {
        //        return Json(true);
        //    }
        //    else
        //    {
        //        return Json($"Department {DepartmentName} is already exist in currency table");
        //    }


        //}
        //// GET: Administration/Department
        //[HttpGet]
        //public async Task<IActionResult> Department(int DepartmentId, string btnActionName)
        //{
        //    ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
        //    var totalDepartments = _context.Departments.OrderBy(x=>x.DepartmentName).ToList();
        //    var currentDepartment = totalDepartments.Where(a => a.DepartmentId == DepartmentId).FirstOrDefault();
        //    if (DepartmentId == 0)
        //    {
        //        DepartmentViewModel departmentVM = new DepartmentViewModel()
        //        {
        //            Departments = totalDepartments,
        //            Department = currentDepartment,
        //            BtnActionName = "Create",
        //        };

        //        return View(departmentVM);
        //    }
        //    else
        //    {
        //        List<Department> lstDepartments = new List<Department>();
        //        lstDepartments = await _context.Departments.Where(a => a.DepartmentId == DepartmentId).ToListAsync();
        //        DepartmentViewModel departmentVm = new DepartmentViewModel()
        //        {
        //            Departments = totalDepartments,
        //            Department = lstDepartments.FirstOrDefault(),
        //            BtnActionName = btnActionName,
        //        };
        //        return View(departmentVm);
        //    }

        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Department(DepartmentViewModel model,DateTime cDate,DateTime uDate)
        //{
        //    var totalDepartments = await _context.Departments.OrderBy(a=>a.DepartmentName).ToListAsync();
        //    model.Departments = totalDepartments;
        //    var DepartmentNameExist = await _context.Departments.Where(x => x.DepartmentName == model.Department.DepartmentName).FirstOrDefaultAsync();
        //    if (ModelState.IsValid)
        //    {
        //        cDate = DateTime.UtcNow.AddMinutes(390);
        //        uDate = DateTime.UtcNow.AddMinutes(390);
        //        model.Department.CreatedDate = cDate;
        //        if (DepartmentNameExist == null && model.BtnActionName != "Delete")
        //        {
        //            if (model.BtnActionName == "Edit" && DepartmentNameExist == null)
        //            {
        //                try
        //                {
        //                    var dati = _context.Departments.Where(p => p.DepartmentId == model.Department.DepartmentId).Single();
        //                    dati.DepartmentName = model.Department.DepartmentName;
        //                    dati.UpdatedDate = uDate;
        //                    await _context.SaveChangesAsync();
        //                    TempData["SuccessedAlertMessage"] = "Update Successful";
        //                    return RedirectToAction(nameof(Department));
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    if (!DepartmentExists(model.Department.DepartmentId))
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
        //                model.Department.CreatedDate = cDate;
        //                model.Department.UpdatedDate = uDate;
        //                await _context.Departments.AddAsync(model.Department);
        //                await _context.SaveChangesAsync();
        //                TempData["SuccessedAlertMessage"] = "Save Successful";
        //                return RedirectToAction(nameof(Department));
        //            }
        //        }
        //        else
        //        {
        //            if (model.BtnActionName == "Delete")
        //            {
        //                //Department department = await _context.Departments
        //                //.FindAsync(model.Department.DepartmentId);
        //                //_context.Departments.Remove(department);
        //                //await _context.SaveChangesAsync();
        //                //TempData["SuccessedAlertMessage"] = "Delete Successful";
        //                return RedirectToAction(nameof(Department));
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    ModelState.AddModelError("Department.DepartmentName", "Department Name already exist");
        //                    return View(model);
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    if (!DepartmentExists(model.Department.DepartmentId))
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

        //        var NameExist = await _context.Departments.Where(x => x.DepartmentName == model.Department.DepartmentName).FirstOrDefaultAsync();
        //        if (NameExist != null)
        //        {
        //            ModelState.AddModelError("Department.DepartmentName", "Department Name already exist");

        //            return View(model);
        //        }
        //        else
        //        {
        //            model.Department.CreatedDate = cDate;
        //            model.Department.UpdatedDate = uDate;
        //            await _context.Departments.AddAsync(model.Department);
        //            await _context.SaveChangesAsync();
        //            ModelState.AddModelError("Department.DepartmentName", "Department Name can't create!");
        //            return RedirectToAction(nameof(Department));
        //        }
        //    }
        //}
        //[HttpPost]
        //private bool DepartmentNameExists(string DepartmentName)
        //{
        //    return _context.Departments.Any(e => e.DepartmentName == DepartmentName);

        //}
        //private bool DepartmentExists(int id)
        //{
        //    return _context.Departments.Any(e => e.DepartmentId == id);
        //}
        //public JsonResult Add(string name)
        //{
        //    DateTime cDate = DateTime.UtcNow.AddMinutes(390);
        //    DateTime uDate = DateTime.UtcNow.AddMinutes(390);
        //    Department department = new Department();
        //    department.DepartmentName = name;
        //    department.CreatedDate = cDate;
        //    department.UpdatedDate = uDate;
        //    _context.Departments.Add(department);
        //    _context.SaveChanges();

        //    var DepartmentList = _context.Departments.ToList();
        //    return Json(DepartmentList.LastOrDefault());
        //}
        //public async Task<ActionResult> DeleteDepartment(int id)
        //{
        //    var current = _context.Departments.Where(u => u.DepartmentId == id).FirstOrDefault();
        //    _context.Remove(current);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Department));
        //}

    }
}
