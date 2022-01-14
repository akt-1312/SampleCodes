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
using System.Runtime.InteropServices;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class TownshipController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        public TownshipViewModel CountryStateTownshipViewModel { get; set; }
        public TownshipController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            CountryStateTownshipViewModel = new TownshipViewModel()
            {
                //Township = new Township(),
                CountryList = _context.Countries.ToList(),
                StateList = new List<State>(),
                TownshipList = new List<Township>(),
            };

        }
        public JsonResult GetStates(int CountryId)
        {
            List<State> lstState = new List<State>();
            lstState = (from data in _context.States where data.Country.Cty_id == CountryId select data).ToList();

            return Json(new SelectList(lstState, "State_id", "State_name"));
        }



        // GET: CommonAjaxCall/Township
        [Authorize(Policy = "TownshipView")]

        [HttpGet]
        public async Task<IActionResult> Township()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            TownshipViewModel model = new TownshipViewModel();
            var totalTownships = _context.Townships.Include(x => x.State).ThenInclude(x => x.Country).OrderBy(a => a.Tsp_name).ToList();
            model.CountryList = await _context.Countries.OrderBy(x => x.Cty_name).ToListAsync();
            model.TownshipList = totalTownships;
            return View(model);
           

        }
        [Authorize(Policy = "TownshipView")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Township(TownshipViewModel model, string btnSubmit,int toDeleteId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalTownships = _context.Townships.Include(x => x.State).ThenInclude(x => x.Country).OrderBy(a => a.Tsp_name).ToList();
            var total = await _context.Townships.OrderBy(a => a.Tsp_name).ToListAsync();
            model.CountryList = await _context.Countries.OrderBy(x => x.Cty_name).ToListAsync();
            model.TownshipList = totalTownships;
            Township township = new Township();
            btnSubmit = btnSubmit.ToLower().Trim();

            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "TownshipDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var delete = await _context.Townships.FindAsync(toDeleteId);
                try
                {
                    if (delete != null)
                    {
                        _context.Townships.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(Township));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Township Name ({delete.Tsp_name}) can't delete!.These table are use in another Table");
                    return View(model);
                }
            }

            if (ModelState.IsValid)
            {
                //var NameExist = await _context.Townships.Where(x => x.Tsp_name == model.Township.Tsp_name).FirstOrDefaultAsync();
                if (btnSubmit == "update")
                {
                    try
                    {
                        if (model.Tsp_id == null || model.Tsp_id.Value == 0)
                        {
                            return View("Error");
                        }
                        else
                        {
                            if (await TownshipExists(townshipName: model.Tsp_name, townshipId: model.Tsp_id))
                            {
                                ModelState.AddModelError("Tsp_name", "Township Name already exist");
                                return View(model);

                            }
                            else
                            {
                                if (!(await authorizationService.AuthorizeAsync(User, "TownshipUpdate")).Succeeded)
                                {
                                    return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                }
                                var updatedTownship = await _context.Townships.FindAsync(model.Tsp_id.Value);
                                if (updatedTownship != null)
                                {
                                    updatedTownship.Tsp_name = model.Tsp_name;
                                    updatedTownship.StateId = model.StateId.Value;
                                    updatedTownship.UpdatedDate = UpdatedDate;
                                    _context.Townships.Update(updatedTownship);
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
                    if (await TownshipExists(townshipName: model.Tsp_name, townshipId: null))
                    {
                        ModelState.AddModelError("Tsp_name", "Township Name already exist");
                        return View(model);
                    }
                    else
                    {
                        if (!(await authorizationService.AuthorizeAsync(User, "TownshipCreate")).Succeeded)
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                        }
                        township.Tsp_name = model.Tsp_name;
                        township.StateId = model.StateId.Value;
                        township.CreatedDate = CreatedDate;
                        await _context.Townships.AddAsync(township);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Save Successful";


                    }
                }
                return RedirectToAction(nameof(Township));
            }
            else
            {
                ModelState.AddModelError("Tsp_name", "Township Name can't create!");

                return View(model);

            }



        }
        private async Task<bool> TownshipExists(string townshipName, int? townshipId)
        {
            var township = new Township();
            if (townshipId == null || townshipId.Value == 0)
            {
                township = await _context.Townships
                    .Where(x => x.Tsp_name == townshipName).FirstOrDefaultAsync();
            }
            else
            {
                township = await _context.Townships
                    .Where(x => x.Tsp_id != townshipId && x.Tsp_name == townshipName).FirstOrDefaultAsync();
            }
            if (township == null)
            {
                return false;
            }
            else
            {
                return true;
            }
            //return _context.Townships.Any(e => e.Tsp_id == id);
        } [HttpPost]
        private bool TownshipNameExists(string Tsp_name, int StateId)
        {
            return _context.Townships.Any(e => e.Tsp_name == Tsp_name && e.StateId == StateId);

        }

       
    }
}


