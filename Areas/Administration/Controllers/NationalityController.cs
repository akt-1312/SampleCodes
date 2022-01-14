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
    public class NationalityController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        public NationalityViewModel NationalityViewModel { get; set; }
        public NationalityController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            NationalityViewModel = new NationalityViewModel()
            {
                Nationality = new Nationality(),
                NationalityList = new List<Nationality>(),
            };
        }

        // GET: Administration/Nationality
        [Authorize(Policy = "NationalityView")]

        [HttpGet]
        public IActionResult Nationality()
        {
           
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            NationalityViewModel model = new NationalityViewModel();
            var total = _context.Nationalities.OrderBy(a => a.NationalityName).ToList();
            model.NationalityList = total;
             return View(model);
         

        }
        [Authorize(Policy = "NationalityView")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nationality(NationalityViewModel model,string btnSubmit,int toDeleteId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            btnSubmit = btnSubmit.ToLower().Trim();
            Nationality nationality = new Nationality();
            var totalNationality = await _context.Nationalities.OrderBy(a => a.NationalityName).ToListAsync();
            model.NationalityList = totalNationality;
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "NationalityDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var deleteNationality = await _context.Nationalities.FindAsync(toDeleteId);
                try
                {
                    if (deleteNationality != null)
                    {
                        _context.Nationalities.Remove(deleteNationality);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(Nationality));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Nationality Name ({deleteNationality.NationalityName}) can't delete!.These table are use in another Table");
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
                            if (model.Nation_Id == null || model.Nation_Id.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await NationalityExist(nationalityName: model.NationalityName, nationalityId: model.Nation_Id))
                                {
                                    ModelState.AddModelError("NationalityName", "Nationality Name   already exist");
                                    return View(model);

                                }
                                else
                                {
                                    if (!(await authorizationService.AuthorizeAsync(User, "NationalityUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }
                                    var updated = await _context.Nationalities.FindAsync(model.Nation_Id.Value);
                                    updated.NationalityName = model.NationalityName;
                                    updated.UpdatedDate = UpdatedDate;

                                    await _context.SaveChangesAsync();
                                    TempData["SuccessedAlertMessage"] = "Update Successful";

                                    return RedirectToAction(nameof(Nationality));
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
                        if (await NationalityExist(nationalityName: model.NationalityName, nationalityId: null))
                        {
                            ModelState.AddModelError("NationalityName", "Nationality Name  already exist");
                            return View(model);
                        }
                        else
                        {
                            if (!(await authorizationService.AuthorizeAsync(User, "NationalityCreate")).Succeeded)
                            {
                                return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                            }
                            nationality.NationalityName = model.NationalityName;
                            nationality.CreatedDate = CreatedDate;
                            await _context.Nationalities.AddAsync(nationality);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";

                            return RedirectToAction(nameof(Nationality));
                        }

                    }
                }

                else
                {

                    ModelState.AddModelError("NationalityName", "NationalityName can't create!");

                    return View(model);
                }
            }
          
        }
        
        private async Task<bool> NationalityExist(string nationalityName, int? nationalityId)
        {
            var nationality = new Nationality();
            if (nationalityId == null || nationalityId.Value == 0)
            {
                nationality = await _context.Nationalities
                    .Where(x => x.NationalityName == nationalityName).FirstOrDefaultAsync();
            }
            else
            {
                nationality = await _context.Nationalities
                    .Where(x => x.Nation_Id != nationalityId && x.NationalityName == nationalityName).FirstOrDefaultAsync();
            }
            if (nationality == null)
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
