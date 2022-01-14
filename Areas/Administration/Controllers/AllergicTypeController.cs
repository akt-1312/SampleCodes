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
    [Authorize]
    public class AllergicTypeController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        [BindProperty]
        public AllergicTypeViewModel allergicTypeViewModel { get; set; }
        public AllergicTypeController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
           this.authorizationService = authorizationService;
            _context = context;
            allergicTypeViewModel = new AllergicTypeViewModel()
            {
                AllergicType = new AllergicType(),
                AllergicTypeList = new List<AllergicType>(),
            };

        }

        // GET: Administration/Gender
        [Authorize(Policy = "AllergicTypeView")]
        [HttpGet, ActionName("AllergicType")]
        public IActionResult AllergicType()
        {
            
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            AllergicTypeViewModel model = new AllergicTypeViewModel();

            var total = _context.AllergicTypes.OrderBy(a => a.AllergicTypeName).ToList();
            model.AllergicTypeList = total;
            return View(model);

        }

        [Authorize(Policy = "AllergicTypeView")]

        [HttpPost, ActionName("AllergicType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AllergicType(AllergicTypeViewModel model,string btnSubmit,int toDeleteId)

        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            AllergicType allergicType = new AllergicType();
            btnSubmit = btnSubmit.ToLower().Trim();
            var total = await _context.AllergicTypes.OrderBy(a => a.AllergicTypeName).ToListAsync();
            model.AllergicTypeList = total;
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "AllergicTypeDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var deleteAllergicType= await _context.AllergicTypes.FindAsync(toDeleteId);
                try
                {
                    if (deleteAllergicType != null)
                    {
                        _context.AllergicTypes.Remove(deleteAllergicType);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(AllergicType));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"AllergicType Name ({deleteAllergicType.AllergicTypeName}) can't delete!.These table are use in another Table");
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
                            if (model.AllergicTypeId == null || model.AllergicTypeId.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                //if (await AllergicTypeExist(allergictypeName: model.AllergicTypeName, allergictypeId: model.AllergicTypeId))
                                //{
                                //    ModelState.AddModelError("AllergicTypeName", "AllergicType Name  already exist");
                                //    return View(model);

                                //}
                                //else
                                //{
                                    if (!(await authorizationService.AuthorizeAsync(User, "AllergicTypeUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }
                                    var updated = await _context.AllergicTypes.FindAsync(model.AllergicTypeId.Value);
                                    if (updated != null)
                                    {
                                        updated.AllergicTypeName = model.AllergicTypeName;
                                        updated.UpdatedDate = UpdatedDate;
                                        _context.AllergicTypes.Update(updated);
                                        await _context.SaveChangesAsync();
                                        TempData["SuccessedAlertMessage"] = "Update Successful";
                                        return RedirectToAction(nameof(AllergicType));

                                    }
                                    else
                                    {
                                        return View("Error");
                                    }

                                }
                            //}



                        }
                        catch (DbUpdateConcurrencyException)
                        {

                            throw;

                        }


                    }

                    else
                    {


                        if (await AllergicTypeExist(allergictypeName: model.AllergicTypeName, allergictypeId: null))
                        {
                            ModelState.AddModelError("AllergicTypeName", "AllergicType Name already exist");
                            return View(model);
                        }
                        else
                        {
                            if (!(await authorizationService.AuthorizeAsync(User, "AllergicTypeCreate")).Succeeded)
                            {
                                return RedirectToAction("AccessDenied", "UserAccount", "Administration");
                            }

                            allergicType.AllergicTypeName = model.AllergicTypeName;
                            allergicType.CreatedDate = CreatedDate;
                            await _context.AllergicTypes.AddAsync(allergicType);

                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";
                        }

                    }

                    //}
                    return RedirectToAction(nameof(AllergicType));


                }
                else
                {
                    ModelState.AddModelError("AllergicTypeName", "AllergicTypeName can't create!");

                    return View(model);
                }
            }
            
        }


        private async Task<bool> AllergicTypeExist(string allergictypeName, int? allergictypeId)
        {
            var allergic = new AllergicType();
            if (allergictypeId == null || allergictypeId.Value == 0)
            {
                allergic = await _context.AllergicTypes
                    .Where(x => x.AllergicTypeName == allergictypeName).FirstOrDefaultAsync();
            }
            else
            {
                allergic = await _context.AllergicTypes
                    .Where(x => x.AllergicTypeId != allergictypeId && x.AllergicTypeName == allergictypeName).FirstOrDefaultAsync();
            }
            if (allergic == null)
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
