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
    public class OccupationController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        public OccupationViewModel OccupationViewModel { get; set; }
        public OccupationController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            OccupationViewModel = new OccupationViewModel()
            {
                Occupation = new Occupation(),
                OccupationList = new List<Occupation >(),
            };
        }

        // GET: Administration/Occupation
        [Authorize(Policy = "OccupationView")]

        [HttpGet]
        public IActionResult Occupation()
        {
           

            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            OccupationViewModel model = new OccupationViewModel();
            var total = _context.Occupations.OrderBy(a => a.OccupationName).ToList();
            model.OccupationList = total;
            return View(model);
          
        }
        [Authorize(Policy = "OccupationView")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Occupation(OccupationViewModel model,string btnSubmit,int toDeleteId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalOccupation = await _context.Occupations.OrderBy(a => a.OccupationName).ToListAsync();
            model.OccupationList = totalOccupation;
            btnSubmit = btnSubmit.ToLower().Trim();
            Occupation occupation = new Occupation();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "OccupationDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var deleteOccupation= await _context.Occupations.FindAsync(toDeleteId);
                try
                {
                    if (deleteOccupation != null)
                    {
                        _context.Occupations.Remove(deleteOccupation);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(Occupation));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Occupation Name ({deleteOccupation.OccupationName}) can't delete!.These table are use in another Table");
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
                            if (model.Occu_Id == null || model.Occu_Id.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await OccupationExist(occupationName: model.OccupationName, occupationId: model.Occu_Id))
                                {
                                    ModelState.AddModelError("OccupationName", "Occupation Name   already exist");
                                    return View(model);

                                }
                                else
                                {
                                    if (!(await authorizationService.AuthorizeAsync(User, "OccupationUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }
                                    var updated = await _context.Occupations.FindAsync(model.Occu_Id.Value);
                                    updated.OccupationName = model.OccupationName;
                                    updated.UpdatedDate = UpdatedDate;

                                    await _context.SaveChangesAsync();
                                    TempData["SuccessedAlertMessage"] = "Update Successful";

                                    return RedirectToAction(nameof(Occupation));
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
                        if (await OccupationExist(occupationName: model.OccupationName, occupationId:null))
                        {
                            ModelState.AddModelError("OccupationName", "Occupation Name   already exist");
                            return View(model);

                        }
                        else
                        {
                            if (!(await authorizationService.AuthorizeAsync(User, "OccupationCreate")).Succeeded)
                            {
                                return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                            }
                            occupation.OccupationName = model.OccupationName;
                            occupation.CreatedDate = CreatedDate;
                            await _context.Occupations.AddAsync(occupation);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";

                            return RedirectToAction(nameof(Occupation));
                        }

                    }
                }

                else
                {

                    ModelState.AddModelError("OccupationName", "OccupationName can't create!");

                    return View(model);
                }
            }
           
        }
      
        private async Task<bool> OccupationExist(string occupationName, int? occupationId)
        {
            var occupation = new Occupation();
            if (occupationId == null || occupationId.Value == 0)
            {
                occupation = await _context.Occupations
                    .Where(x => x.OccupationName == occupationName).FirstOrDefaultAsync();
            }
            else
            {
                occupation = await _context.Occupations
                    .Where(x => x.Occu_Id != occupationId && x.OccupationName == occupationName).FirstOrDefaultAsync();
            }
            if (occupation == null)
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

