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
    public class GeneralServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService authorizationService;
        public GeneralServiceViewModel GeneralServiceVM { get; set; }

        public GeneralServiceController(ApplicationDbContext context,
                                        IAuthorizationService authorizationService)
        {
            _context = context;
            this.authorizationService = authorizationService;
            GeneralServiceVM = new GeneralServiceViewModel()
            {
                GeneralServices = new List<GeneralService>(),
            };
        }
        [HttpGet]
        [Authorize(Policy = "GeneralServiceView")]
        public async Task<IActionResult> GeneralService()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            GeneralServiceViewModel model = new GeneralServiceViewModel();
            model.GeneralServices = await _context.GeneralServices.OrderBy(x => x.GeneralServiceDescription).ToListAsync();
            return View(model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GeneralServiceView")]
        public async Task<IActionResult> GeneralService(GeneralServiceViewModel model, string btnSubmit,int toDeleteGSId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            model.GeneralServices = await _context.GeneralServices.OrderBy(x => x.GeneralServiceDescription).ToListAsync();
            GeneralService gs = new GeneralService();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "GeneralServiceDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var delete = await _context.GeneralServices.FindAsync(toDeleteGSId);
                try
                {
                    if (delete != null)
                    {
                        _context.GeneralServices.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(GeneralService));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"General Service ({delete.GeneralServiceDescription}) can't delete!.These table are use in another Table");
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
                        if ((await authorizationService.AuthorizeAsync(User, "GeneralServiceUpdate")).Succeeded)
                        {
                            try
                            {
                                if (model.GeneralServiceId == null || model.GeneralServiceId.Value == 0)
                                {
                                    return View("Error");
                                }
                                else
                                {
                                    //if (await GServiceExists(GeneralServiceDescription: model.GeneralServiceDescription, GeneralServiceId: model.GeneralServiceId))
                                    //{
                                    //    ModelState.AddModelError("GeneralServiceDescription", "Description already exist");
                                    //    return View(model);

                                    //}
                                    //else
                                    //{
                                        var updated = await _context.GeneralServices.FindAsync(model.GeneralServiceId.Value);
                                        if (updated != null)
                                        {
                                            updated.GeneralServiceDescription = model.GeneralServiceDescription;
                                            updated.GeneralServicePrice = model.GeneralServicePrice.Value;
                                            updated.UpdatedDate = UpdatedDate;
                                            _context.GeneralServices.Update(updated);
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
                        if ((await authorizationService.AuthorizeAsync(User, "GeneralServiceUpdate")).Succeeded)
                        {
                            //if (await GServiceExists(GeneralServiceDescription: model.GeneralServiceDescription, GeneralServiceId: model.GeneralServiceId))
                            //{
                            //    ModelState.AddModelError("GeneralServiceDescription", "Description already exist");
                            //    return View(model);
                            //}
                            //else
                            //{
                                gs.GeneralServiceDescription = model.GeneralServiceDescription;
                                gs.GeneralServicePrice = model.GeneralServicePrice.Value;
                                gs.CreatedDate = CreatedDate;
                                await _context.GeneralServices.AddAsync(gs);
                                await _context.SaveChangesAsync();
                                TempData["SuccessedAlertMessage"] = "Save Successful";
                            //}
                        }
                        else
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                        }
                    }
                    return RedirectToAction(nameof(GeneralService));
                }
                else
                {
                    ModelState.AddModelError("GeneralService.GeneralServiceDescription", "General Service can't create!");

                    return View(model);

                }
            }
        }
        //[HttpPost]
        //[Authorize(Policy = "GeneralServiceDelete")]
        //public async Task<IActionResult> DeleteGeneralService(int? GeneralServiceId)
        //{
        //    ModelState.AddModelError("GeneralService.GeneralServiceDescription", "General Service already exist");
        //    GeneralServiceViewModel model = new GeneralServiceViewModel();
        //    model.GeneralServices = await _context.GeneralServices.OrderBy(x => x.GeneralServiceDescription).ToListAsync();
        //    var deletegs = await _context.GeneralServices.FindAsync(GeneralServiceId);
        //    try
        //    {

        //        if (deletegs != null)
        //        {
        //            _context.GeneralServices.Remove(deletegs);
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
        //        ModelState.AddModelError("", $"General Service ({deletegs.GeneralServiceDescription}) can't delete!.These table are use in another Table");
        //        return View("GeneralService", model);
        //    }
        //    return RedirectToAction(nameof(GeneralService));

        //    //return View();
        //}

        //private async Task<bool> GServiceExists(string GeneralServiceDescription,int? GeneralServiceId)
        //{
        //    var gs = new GeneralService();
        //    if (GeneralServiceId != null)
        //    {
        //          gs = await _context.GeneralServices
        //            .Where(x => x.GeneralServiceDescription == GeneralServiceDescription && x.GeneralServiceId!=GeneralServiceId).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        gs = await _context.GeneralServices
        //           .Where(x => x.GeneralServiceDescription == GeneralServiceDescription ).FirstOrDefaultAsync();
        //    }
        //    if (gs == null)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }

        //}
        //[HttpGet]
        //public async Task<IActionResult> GeneralService(int GeneralServiceId, string btnActionName)
        //{
        //    ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
        //    var totalGeneralServices = _context.GeneralServices.ToList();
        //    var currentGeneralService = totalGeneralServices.Where(a => a.GeneralServiceId == GeneralServiceId).FirstOrDefault();
        //    if (GeneralServiceId == 0)
        //    {
        //        GeneralServiceViewModel generalServiceVM = new GeneralServiceViewModel()
        //        {
        //            GeneralServices = totalGeneralServices,
        //            GeneralService = currentGeneralService,
        //            BtnActionName = "Create",
        //        };

        //        return View(generalServiceVM);
        //    }
        //    else
        //    {
        //        List<GeneralService> lstGeneralServices = new List<GeneralService>();
        //        lstGeneralServices = await _context.GeneralServices.Where(a => a.GeneralServiceId == GeneralServiceId).ToListAsync();
        //        GeneralServiceViewModel generalServiceVm = new GeneralServiceViewModel()
        //        {
        //            GeneralServices = lstGeneralServices,
        //            GeneralService = lstGeneralServices.FirstOrDefault(),
        //            BtnActionName = btnActionName,
        //        };
        //        return View(generalServiceVm);
        //    }

        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> GeneralService(GeneralServiceViewModel model, DateTime cDate, DateTime uDate)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        cDate = DateTime.UtcNow.AddMinutes(390);
        //        uDate = DateTime.UtcNow.AddMinutes(390);
        //        var totalGeneralServices = await _context.GeneralServices.ToListAsync();

        //        if (model.BtnActionName != "Delete")
        //        {
        //            if (model.BtnActionName == "Edit")
        //            {
        //                try
        //                {
        //                    var dati = _context.GeneralServices.Where(p => p.GeneralServiceId == model.GeneralService.GeneralServiceId).Single();
        //                    dati.GeneralServiceDescription = model.GeneralService.GeneralServiceDescription;
        //                    dati.GeneralServicePrice = model.GeneralService.GeneralServicePrice;
        //                    dati.UpdatedDate = uDate;
        //                    await _context.SaveChangesAsync();
        //                    TempData["SuccessedAlertMessage"] = "Update Successful";
        //                    return RedirectToAction(nameof(GeneralService));
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    if (!GeneralServiceExists(model.GeneralService.GeneralServiceId))
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
        //                model.GeneralService.CreatedDate = cDate;
        //                await _context.GeneralServices.AddAsync(model.GeneralService);
        //                await _context.SaveChangesAsync();
        //                TempData["SuccessedAlertMessage"] = "Save Successful";
        //                return RedirectToAction(nameof(GeneralService));
        //            }
        //        }
        //        else
        //        {
        //            if (model.BtnActionName == "Delete")
        //            {
        //                GeneralService generalService = await _context.GeneralServices
        //                    .FindAsync(model.GeneralService.GeneralServiceId);
        //                _context.GeneralServices.Remove(generalService);
        //                await _context.SaveChangesAsync();
        //                TempData["SuccessedAlertMessage"] = "Delete Successful";
        //                return RedirectToAction(nameof(GeneralService));
        //            }
        //            else
        //            {
        //                try
        //                {
        //                    ModelState.AddModelError("GeneralService.GeneralServiceDescription", "GeneralService Name already exist");
        //                    GeneralServiceViewModel generalServiceVm = new GeneralServiceViewModel()
        //                    {
        //                        GeneralServices = totalGeneralServices,
        //                    };
        //                    return View(generalServiceVm);
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    if (!GeneralServiceExists(model.GeneralService.GeneralServiceId))
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
        //        //ModelState.AddModelError("Currency.CurrencyName", "Currency Name can't create!");
        //        return View(model);
        //    }
        //}
        //[HttpPost]
        //private bool GeneralServiceNameExists(string GeneralServiceDescription)
        //{
        //    return _context.GeneralServices.Any(e => e.GeneralServiceDescription == GeneralServiceDescription);

        //}
        //private bool GeneralServiceExists(int id)
        //{
        //    return _context.GeneralServices.Any(e => e.GeneralServiceId == id);
        //}
    }
}
