using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Administration;
using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class ReferalHospitalController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        public ReferalHospitalViewModel ReferalHospitalViewModel { get; set; }
        public ReferalHospitalController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            ReferalHospitalViewModel = new ReferalHospitalViewModel()
            {
                ReferalHospital = new ReferalHospital(),
                ReferalHospitalList = new List<ReferalHospital>(),
            };
        }


        [Authorize(Policy = "ReferalHospitalView")]

        [HttpGet]
        public IActionResult ReferalHospital()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            ReferalHospitalViewModel model = new ReferalHospitalViewModel();
            var total = _context.ReferalHospitals.OrderBy(a => a.HospitalName).ToList();
            model.ReferalHospitalList = total;
             return View(model);
            

        }

        [Authorize(Policy = "ReferalHospitalView")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReferalHospital(ReferalHospitalViewModel model,string btnSubmit,int toDeleteId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            btnSubmit = btnSubmit.ToLower().Trim();

            var total = await _context.ReferalHospitals.OrderBy(a => a.HospitalName).ToListAsync();
            model.ReferalHospitalList = total;
            ReferalHospital hospital = new ReferalHospital();
            if (btnSubmit == "delete")
            {
                if (!(await authorizationService.AuthorizeAsync(User, "ReferalHospitalDelete")).Succeeded)
                {
                    return RedirectToAction("AccessDenied", "UserAccount", "Administration");
                }

                ModelState.Clear();
                var delete = await _context.ReferalHospitals.FindAsync(toDeleteId);
                try
                {
                    if (delete != null)
                    {
                        _context.ReferalHospitals.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(ReferalHospital));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Hospital Name ({delete.HospitalName}) can't delete!.These table are use in another Table");
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
                            if (model.Id == null || model.Id.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await Exist(hospitalName: model.HospitalName, hospitalId: model.Id))
                                {
                                    ModelState.AddModelError("HospitalName", "Hospital Name  already exist");
                                    return View(model);

                                }
                                else
                                {
                                    if (!(await authorizationService.AuthorizeAsync(User, "ReferalHospitalUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }
                                    try
                                    {
                                        if (model.HospitalName == null)
                                        {
                                            if (model.ReferalHospital.HospitalName == null)
                                            {
                                                ModelState.AddModelError("HospitalName", "ReferalHospital  Name is reqired.");
                                            }
                                            return View(model);
                                        }
                                        else
                                        {
                                            var updated = await _context.ReferalHospitals.FindAsync(model.Id.Value);
                                            updated.HospitalName = model.HospitalName;
                                            updated.UpdatedDate = UpdatedDate;

                                            await _context.SaveChangesAsync();
                                            TempData["SuccessedAlertMessage"] = "Update Successful";

                                        }

                                        return RedirectToAction(nameof(ReferalHospital));
                                    }
                                    catch
                                    {
                                        throw;
                                    }
                                   
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
                            if (model.HospitalName == null)
                            {
                                if (model.ReferalHospital.HospitalName == null)
                                {
                                    ModelState.AddModelError("HospitalName", "Hospital  Name is reqired.");
                                }
                                return View(model);
                            }
                            else
                            {
                            if (await Exist(hospitalName: model.HospitalName, hospitalId: null))
                            {
                                ModelState.AddModelError("HospitalName", "Hospital Name  already exist");
                                return View(model);
                            }
                            else
                            {
                                if (!(await authorizationService.AuthorizeAsync(User, "ReferalHospitalCreate")).Succeeded)
                                {
                                    return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                }
                                hospital.HospitalName = model.HospitalName;
                                hospital.CreatedDate = CreatedDate;
                                await _context.ReferalHospitals.AddAsync(hospital);
                                await _context.SaveChangesAsync();
                                TempData["SuccessedAlertMessage"] = "Save Successful";
                            }
                            return RedirectToAction(nameof(ReferalHospital));


                            }

                        }
                  
                   
                      }
                else
                {


                    ModelState.AddModelError("HospitalName", "HospitalName can't create!");

                    return View(model);
                }
            }
          
        }
        private async Task<bool> Exist(string hospitalName, int? hospitalId)
        {
            var hospital = new ReferalHospital();
            if (hospitalId == null || hospitalId.Value == 0)
            {
                hospital = await _context.ReferalHospitals
                    .Where(x => x.HospitalName == hospitalName).FirstOrDefaultAsync();
            }
            else
            {
                hospital = await _context.ReferalHospitals
                    .Where(x => x.Id != hospitalId && x.HospitalName == hospitalName).FirstOrDefaultAsync();
            }
            if (hospital == null)
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