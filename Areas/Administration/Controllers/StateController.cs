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
using HMS.Extensions;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class StateController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        public StateViewModel StateViewModel { get; set; }
        public StateController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            StateViewModel = new StateViewModel()
            {

                CountryList = _context.Countries.ToList(),
                StateList = new List<State>(),
            };

        }




        [Authorize(Policy = "StateView")]

        [HttpGet]
        public async Task<IActionResult> State()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            StateViewModel model = new StateViewModel();
            var totalStates = _context.States.Include(x => x.Country).OrderBy(a => a.Country.Cty_name).ToList();
            //var totalStates = _context.States.Include(x => x.Country).OrderBy(a => a.Country.Cty_name).ToList();

            model.CountryList = await _context.Countries.OrderBy(x => x.Cty_name).ToListAsync();
            model.StateList = totalStates;
            return View(model);


        }
        [Authorize(Policy = "StateView")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> State(StateViewModel model, string btnSubmit,int ToDeleteId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalStates = _context.States.Include(x => x.Country).OrderBy(a => a.State_name).ToList();
            var total = await _context.States.OrderBy(a => a.State_name).ToListAsync();
            model.CountryList = await _context.Countries.OrderBy(x => x.Cty_name).ToListAsync();
            model.StateList = totalStates;
            State state = new State();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "StateDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var deleteUnit = await _context.States.FindAsync(ToDeleteId);
                try
                {
                    if (deleteUnit != null)
                    {
                        _context.States.Remove(deleteUnit);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(State));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"State Name ({deleteUnit.State_name}) can't delete!.These table are use in another Table");
                    return View(model);
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (btnSubmit == "update")
                    {
                        //if ((await authorizationService.AuthorizeAsync(User, "StateUpdate")).Succeeded)
                        //{
                        try
                        {
                            if (model.State_id == null || model.State_id.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await StateExists(stateName: model.State_name, stateId: model.State_id))
                                {
                                    ModelState.AddModelError("StateName", "State Name already exist");
                                    return View(model);

                                }
                                else
                                {
                                    //if ((await authorizationService.AuthorizeAsync(User, "StateUpdate")).Succeeded)
                                    //{
                                    //    return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    //}
                                    if (!(await authorizationService.AuthorizeAsync(User, "StateUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }
                                    var updatedTownship = await _context.States.FindAsync(model.State_id.Value);
                                    if (updatedTownship != null)
                                    {
                                        updatedTownship.State_name = model.State_name;
                                        updatedTownship.CountryId = model.CountryId;
                                        updatedTownship.UpdatedDate = UpdatedDate;
                                        _context.States.Update(updatedTownship);
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
                        if ((await authorizationService.AuthorizeAsync(User, "StateCreate")).Succeeded)
                        {
                            if (await StateExists(stateName: model.State_name, stateId: null))
                            {
                                ModelState.AddModelError("StateName", "State Name already exist");
                                return View(model);
                            }
                            else
                            {
                                //if ((await authorizationService.AuthorizeAsync(User, "StateCreate")).Succeeded)
                                //{
                                //    return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                //}
                                state.State_name = model.State_name;
                                state.CountryId = model.CountryId;
                                state.CreatedDate = CreatedDate;
                                await _context.States.AddAsync(state);
                                await _context.SaveChangesAsync();
                                TempData["SuccessedAlertMessage"] = "Save Successful";


                            }

                        }
                        else
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });

                        }

                    }
                    return RedirectToAction(nameof(State));

                }
                else
                {
                    ModelState.AddModelError("StateName", "State Name can't create!");

                    return View(model);

                }
            }
       



        }

       


        public JsonResult AddState(string name, int CountryId)
        {
            State state = new State();
            state.State_name = name;
            state.CountryId = CountryId;
            state.CreatedDate = DateTime.UtcNow.AddMinutes(390);
            state.UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            _context.States.Add(state);
            _context.SaveChanges();

            var StateList = _context.States.ToList();
            return Json(StateList.LastOrDefault());
        }
        private async Task<bool> StateExists(string stateName, int? stateId)
        {
            var township = new State();
            if (stateId == null || stateId.Value == 0)
            {
                township = await _context.States
                    .Where(x => x.State_name == stateName).FirstOrDefaultAsync();
            }
            else
            {
                township = await _context.States
                    .Where(x => x.State_id != stateId && x.State_name == stateName).FirstOrDefaultAsync();
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
        }

    }
}
