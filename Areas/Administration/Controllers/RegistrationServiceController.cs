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

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class RegistrationServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService authorizationService;
        public RegistrationServiceViewModel registrationServiceVM { get; set; }

        public RegistrationServiceController(ApplicationDbContext context,
                                             IAuthorizationService authorizationService)
        {
            _context = context;
            this.authorizationService = authorizationService;
            registrationServiceVM = new RegistrationServiceViewModel()
            {
                RegistrationServices = new List<RegistrationService>(),
            };
        }


        [HttpGet]
        [Authorize(Policy = "RegistrationServiceView")]
        public async Task<IActionResult> RegistrationService()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            RegistrationServiceViewModel model = new RegistrationServiceViewModel();
            model.RegistrationServices = await _context.RegistrationServices.OrderBy(x => x.RegistrationServiceDescription).ToListAsync();
            return View(model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "RegistrationServiceView")]
        public async Task<IActionResult> RegistrationService(RegistrationServiceViewModel model, string btnSubmit, int toDeleteRSId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            model.RegistrationServices = await _context.RegistrationServices.OrderBy(x => x.RegistrationServiceDescription).ToListAsync();
            RegistrationService rs = new RegistrationService();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "RegistrationServiceDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var delete = await _context.RegistrationServices.FindAsync(toDeleteRSId);
                try
                {
                    if (delete != null)
                    {
                        _context.RegistrationServices.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(RegistrationService));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"General Service ({delete.RegistrationServiceDescription}) can't delete!.These table are use in another Table");
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
                        if ((await authorizationService.AuthorizeAsync(User, "RegistrationServiceUpdate")).Succeeded)
                        {
                            try
                            {
                                if (model.RegistrationServiceId == null || model.RegistrationServiceId.Value == 0)
                                {
                                    return View("Error");
                                }
                                else
                                {
                                    if (await RServiceExists(RegistrationServiceDescription: model.RegistrationServiceDescription, RegistrationServiceId: model.RegistrationServiceId))
                                    {
                                        ModelState.AddModelError("RegistrationServiceDescription", "Description already exist");
                                        return View(model);

                                    }
                                    else
                                    {
                                        var updated = await _context.RegistrationServices.FindAsync(model.RegistrationServiceId.Value);
                                        if (updated != null)
                                        {
                                            updated.RegistrationServiceDescription = model.RegistrationServiceDescription;
                                            updated.RegistrationServicePrice = model.RegistrationServicePrice.Value;
                                            updated.UpdatedDate = UpdatedDate;
                                            _context.RegistrationServices.Update(updated);
                                            await _context.SaveChangesAsync();
                                            TempData["SuccessedAlertMessage"] = "Update Successful";
                                        }
                                        else
                                        {
                                            return View("Error");
                                        }
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
                        if ((await authorizationService.AuthorizeAsync(User, "RegistrationServiceUpdate")).Succeeded)
                        {
                            if (await RServiceExists(RegistrationServiceDescription: model.RegistrationServiceDescription, RegistrationServiceId: model.RegistrationServiceId))
                            {
                                ModelState.AddModelError("RegistrationServiceDescription", "Description already exist");
                                return View(model);
                            }
                            else
                            {
                                rs.RegistrationServiceDescription = model.RegistrationServiceDescription;
                                rs.RegistrationServicePrice = model.RegistrationServicePrice.Value;
                                rs.CreatedDate = CreatedDate;
                                await _context.RegistrationServices.AddAsync(rs);
                                await _context.SaveChangesAsync();
                                TempData["SuccessedAlertMessage"] = "Save Successful";
                            }
                        }
                        else
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                        }
                    }
                    return RedirectToAction(nameof(RegistrationService));
                }
                else
                {
                    ModelState.AddModelError("RegistrationService.RegistrationServiceDescription", "Registration Service can't create!");

                    return View(model);

                }
            }
        }
        private async Task<bool> RServiceExists(string RegistrationServiceDescription, int? RegistrationServiceId)
        {
            var rs = new RegistrationService();
            if (RegistrationServiceId != null)
            {
                rs = await _context.RegistrationServices
                  .Where(x => x.RegistrationServiceDescription == RegistrationServiceDescription && x.RegistrationServiceId != RegistrationServiceId).FirstOrDefaultAsync();
            }
            else
            {
                rs = await _context.RegistrationServices
                   .Where(x => x.RegistrationServiceDescription == RegistrationServiceDescription).FirstOrDefaultAsync();
            }
            if (rs == null)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        //// GET: Administration/Department
        //[HttpGet]
        //public async Task<IActionResult> RegistrationService(int RegistrationServiceId, string btnActionName)
        //{
        //    ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
        //    var totalRegistrationServices = _context.RegistrationServices.ToList();
        //    var currentRegistrationService = totalRegistrationServices.Where(a => a.RegistrationServiceId == RegistrationServiceId).FirstOrDefault();
        //    if (RegistrationServiceId == 0)
        //    {
        //        RegistrationServiceViewModel registrationServiceVM = new RegistrationServiceViewModel()
        //        {
        //            RegistrationServices = totalRegistrationServices,
        //            RegistrationService = currentRegistrationService,
        //            BtnActionName = "Create",
        //        };

        //        return View(registrationServiceVM);
        //    }
        //    else
        //    {
        //        List<RegistrationService> lstRegistrationServices = new List<RegistrationService>();
        //        lstRegistrationServices = await _context.RegistrationServices.Where(a => a.RegistrationServiceId == RegistrationServiceId).ToListAsync();
        //        RegistrationServiceViewModel registrationServiceVm = new RegistrationServiceViewModel()
        //        {
        //            RegistrationServices = totalRegistrationServices,
        //            RegistrationService = lstRegistrationServices.FirstOrDefault(),
        //            BtnActionName = btnActionName,
        //        };
        //        return View(registrationServiceVm);
        //    }

        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> RegistrationService(RegistrationServiceViewModel model, DateTime cDate, DateTime uDate)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        cDate = DateTime.UtcNow.AddMinutes(390);
        //        uDate = DateTime.UtcNow.AddMinutes(390);
        //        model.RegistrationService.CreatedDate = cDate;
        //        var totalRegistrationServices = await _context.RegistrationServices.ToListAsync();
        //        var RegistrationServiceNameExist = await _context.RegistrationServices.Where(x => x.RegistrationServiceDescription == model.RegistrationService.RegistrationServiceDescription).FirstOrDefaultAsync();
        //        if (RegistrationServiceNameExist == null && model.BtnActionName != "Delete")
        //        {
        //            if (model.BtnActionName == "Edit")
        //            {
        //                try
        //                {
        //                    var dati = _context.RegistrationServices.Where(p => p.RegistrationServiceId == model.RegistrationService.RegistrationServiceId).Single();
        //                    dati.RegistrationServiceDescription = model.RegistrationService.RegistrationServiceDescription;
        //                    dati.UpdatedDate = uDate;
        //                    await _context.SaveChangesAsync();
        //                    TempData["SuccessedAlertMessage"] = "Update Successful";
        //                    return RedirectToAction(nameof(RegistrationService));
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    if (!RegistrationServiceExists(model.RegistrationService.RegistrationServiceId))
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
        //                model.RegistrationService.CreatedDate = cDate;
        //                model.RegistrationService.UpdatedDate = uDate;
        //                await _context.RegistrationServices.AddAsync(model.RegistrationService);
        //                await _context.SaveChangesAsync();
        //                TempData["SuccessedAlertMessage"] = "Save Successful";
        //                return RedirectToAction(nameof(registrationServiceVM));
        //            }
        //        }
        //        else
        //        {
        //            if (model.BtnActionName == "Delete")
        //            {
        //                RegistrationService registrationService = await _context.RegistrationServices
        //                    .FindAsync(model.RegistrationService.RegistrationServiceId);
        //                _context.RegistrationServices.Remove(registrationService);
        //                await _context.SaveChangesAsync();
        //                TempData["SuccessedAlertMessage"] = "Delete Successful";
        //                return RedirectToAction(nameof(RegistrationService));
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    ModelState.AddModelError("RegistrationService.RegistrationServiceDescription", "Registration Service Description already exist");
        //                    RegistrationServiceViewModel registrationServiceVm = new RegistrationServiceViewModel()
        //                    {
        //                        RegistrationServices = totalRegistrationServices,
        //                    };
        //                    return View(registrationServiceVm);
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    if (!RegistrationServiceExists(model.RegistrationService.RegistrationServiceId))
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
        //        ModelState.AddModelError("ReigstrationService.RegistrationServiceDescription", "RegistrationService Description can't create!");
        //        return View(model);
        //    }
        //}
        //[HttpPost]
        //private bool RegistrationServiceNameExists(string RegistrationServiceDescription)
        //{
        //    return _context.RegistrationServices.Any(e => e.RegistrationServiceDescription == RegistrationServiceDescription);

        //}
        //private bool RegistrationServiceExists(int id)
        //{
        //    return _context.RegistrationServices.Any(e => e.RegistrationServiceId == id);
        //}
    }
}
