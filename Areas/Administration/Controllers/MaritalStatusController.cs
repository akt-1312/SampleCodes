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
    public class MaritalStatusController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        public MaritalStatusViewModel MaritalStatusViewModel { get; set; }
        public MaritalStatusController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;

            MaritalStatusViewModel = new MaritalStatusViewModel()
            {
                MaritalStatus = new MaritalStatus(),
                MaritalStatusesList = new List<MaritalStatus>(),
            };
        }

        // GET: Administration/MaritalStatus
        [Authorize(Policy = "MaritalStatusView")]

        [HttpGet]
        public IActionResult MaritalStatus()
        {
           


            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            MaritalStatusViewModel model = new MaritalStatusViewModel();
            var total = _context.MaritalStatuses.OrderBy(a=>a.Marital_Status).ToList();
            model.MaritalStatusesList = total;
             return View(model);
        }
        [Authorize(Policy = "MaritalStatusView")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MaritalStatus(MaritalStatusViewModel model,string btnSubmit,int toDeleteId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var total = await _context.MaritalStatuses.OrderBy(a => a.Marital_Status).ToListAsync();
            model.MaritalStatusesList = total;
            MaritalStatus maritalStatus = new MaritalStatus();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "MaritalStatusDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var deleteMarital = await _context.MaritalStatuses.FindAsync(toDeleteId);
                try
                {
                    if (deleteMarital != null)
                    {
                        _context.MaritalStatuses.Remove(deleteMarital);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(MaritalStatus));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"MaritalStatus Name ({deleteMarital.Marital_Status}) can't delete!.These table are use in another Table");
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
                            if (model.MS_Id == null || model.MS_Id.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await MaritalStatusExist(maritalName: model.Marital_Status, maritalId: model.MS_Id))
                                {
                                    ModelState.AddModelError("Marital_Status", "MaritalStatus Name  already exist in current table");
                                    return View(model);

                                }
                                else
                                {
                                    if (!(await authorizationService.AuthorizeAsync(User, "MaritalStatusUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }
                                    var updated = await _context.MaritalStatuses.FindAsync(model.MS_Id.Value);
                                    updated.Marital_Status = model.Marital_Status;
                                    updated.UpdatedDate = UpdatedDate;

                                    await _context.SaveChangesAsync();
                                    TempData["SuccessedAlertMessage"] = "Update Successful";

                                    return RedirectToAction(nameof(MaritalStatus));
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
                        if (await MaritalStatusExist(maritalName: model.Marital_Status, maritalId: null))
                        {
                            ModelState.AddModelError("Marital_Status", "MaritalStatus Name already exist in current table");
                            return View(model);
                        }
                        else
                        {
                            if (!(await authorizationService.AuthorizeAsync(User, "MaritalStatusCreate")).Succeeded)
                            {
                                return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                            }
                            maritalStatus.Marital_Status = model.Marital_Status;
                            maritalStatus.CreatedDate = CreatedDate;
                            await _context.MaritalStatuses.AddAsync(maritalStatus);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";

                            return RedirectToAction(nameof(MaritalStatus));
                        }

                    }
                }

                else
                {
                    ModelState.AddModelError("Marital_Status", "MaritalStatusName can't create!");

                    return View(model);
                }
            }
           
        }
       
        private async Task<bool> MaritalStatusExist(string maritalName, int? maritalId)
        {
            var marital = new MaritalStatus();
            if (maritalId == null || maritalId.Value == 0)
            {
                marital = await _context.MaritalStatuses
                    .Where(x => x.Marital_Status == maritalName).FirstOrDefaultAsync();
            }
            else
            {
                marital = await _context.MaritalStatuses
                    .Where(x => x.MS_Id != maritalId && x.Marital_Status == maritalName).FirstOrDefaultAsync();
            }
            if (marital == null)
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
