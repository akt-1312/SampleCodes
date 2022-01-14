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
using Microsoft.AspNetCore.Routing;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class GenderController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        //GenderViewModel genderViewModel= new GenderViewModel 
        [BindProperty]
        public GenderViewModel genderViewModel { get; set; }
        public GenderController(ApplicationDbContext context,IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
           genderViewModel = new GenderViewModel()
            { 
                Gender = new Gender(),
                Genders = new List<Gender>(),
            };

        }

        // GET: Administration/Gender
        [Authorize(Policy = "GenderView")]
        [HttpGet]
        public IActionResult Gender()
        {
           
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            GenderViewModel model = new GenderViewModel();
            var totalGenders = _context.Genders.OrderBy(a => a.GenderName).ToList();
            model.Genders = totalGenders;
            return View(model);
           

        }
        [Authorize(Policy = "GenderView")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Gender(GenderViewModel model,string btnSubmit,int toDeleteId)
        
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var total = await _context.Genders.OrderBy(a => a.GenderName).ToListAsync();
            model.Genders = total;
            btnSubmit = btnSubmit.ToLower().Trim();
            Gender gender = new Gender();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "GenderDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var deleteGender= await _context.Genders.FindAsync(toDeleteId);
                try
                {
                    if (deleteGender != null)
                    {
                        _context.Genders.Remove(deleteGender);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(Gender));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Gender Name ({deleteGender.GenderName}) can't delete!.These table are use in another Table");
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
                            if (model.Gender_Id == null || model.Gender_Id.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await GenderExist(genderName: model.GenderName, genderId: model.Gender_Id))
                                {
                                    ModelState.AddModelError("GenderName", "Gender Name  already exist");
                                    return View(model);

                                }
                                else
                                {
                                    if (!(await authorizationService.AuthorizeAsync(User, "GenderUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }

                                    var updated = await _context.Genders.FindAsync(model.Gender_Id.Value);
                                    updated.GenderName = model.GenderName;
                                    updated.UpdatedDate = UpdatedDate;
                                    await _context.SaveChangesAsync();
                                    TempData["SuccessedAlertMessage"] = "Update Successful";

                                    return RedirectToAction(nameof(Gender));

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
                        if (await GenderExist(genderName: model.GenderName, genderId: null))
                        {
                            ModelState.AddModelError("GenderName", "Gender Name already exist");
                            return View(model);
                        }
                        else
                        {
                            if (!(await authorizationService.AuthorizeAsync(User, "GenderCreate")).Succeeded)
                            {
                                return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                            }
                            gender.GenderName = model.GenderName;
                            gender.CreatedDate = CreatedDate;
                            await _context.Genders.AddAsync(gender);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";

                            return RedirectToAction(nameof(Gender));
                        }

                    }
                }
                else
                {

                    ModelState.AddModelError("GenderName", "GenderName  can't create!");

                    return View(model);
                }
            }
         
        }

       
        private async Task<bool> GenderExist(string genderName, int? genderId)
        {
            var gender = new Gender();
            if (genderId == null || genderId.Value == 0)
            {
                gender = await _context.Genders
                    .Where(x => x.GenderName == genderName).FirstOrDefaultAsync();
            }
            else
            {
                gender = await _context.Genders
                    .Where(x => x.Gender_Id != genderId && x.GenderName == genderName).FirstOrDefaultAsync();
            }
            if (gender == null)
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
